namespace Amusing.Mijn.Services;

public sealed partial class ChoirPeople
{
    public async Task<List<OutgoingRoleInvitation>> OutgoingAsync(uint actor, uint group, CancellationToken ct)
    {
        await using var connection = await source.OpenConnectionAsync(ct);
        await using var tx = await connection.BeginTransactionAsync(ct);
        var roles = await LockAsync(connection, tx, group, ct);
        if (!SupportMode.CanSupport(supportContext, actor, group) && !Manages(roles, actor)) return [];

        await using var cmd = connection.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = """
            SELECT i.id,COALESCE(NULLIF(CONCAT_WS(' ',NULLIF(p.voornaam,''),NULLIF(p.tussenvoegsel,''),NULLIF(p.achternaam,'')),''),p.email,''),
                i.role_name,i.expires_at,COALESCE(p.actief,0)=1 AND NULLIF(w.hash,'') IS NOT NULL
            FROM mijn_role_invitations i
            LEFT JOIN ah_personen p ON p.persoon_id=i.person_id
            LEFT JOIN ah_personen_wachtwoorden w ON w.id=p.persoon_id
            WHERE i.group_id=@group AND i.accepted_at IS NULL AND i.cancelled_at IS NULL
            ORDER BY i.created_at DESC LIMIT 100
            """;
        Add(cmd, "@group", group);
        var invitations = new List<OutgoingRoleInvitation>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
            invitations.Add(new(reader.GetString(0), reader.GetString(1), reader.GetString(2),
                DateTime.SpecifyKind(reader.GetDateTime(3), DateTimeKind.Utc), reader.GetBoolean(4)));
        return invitations;
    }

    public async Task<PeopleResult> CancelInvitationAsync(uint actor, uint group, string invitation, CancellationToken ct)
    {
        if (!Guid.TryParseExact(invitation, "N", out _)) return PeopleResult.Invalid;
        await using var connection = await source.OpenConnectionAsync(ct);
        await using var tx = await connection.BeginTransactionAsync(ct);
        var roles = await LockAsync(connection, tx, group, ct);
        if (!SupportMode.CanSupport(supportContext, actor, group) && !Manages(roles, actor)) return PeopleResult.Forbidden;
        uint target;
        string role;
        await using (var read = connection.CreateCommand())
        {
            read.Transaction = tx;
            read.CommandText = "SELECT person_id,role_name FROM mijn_role_invitations WHERE id=@id AND group_id=@group AND accepted_at IS NULL AND cancelled_at IS NULL FOR UPDATE";
            Add(read, "@id", invitation); Add(read, "@group", group);
            await using var reader = await read.ExecuteReaderAsync(ct);
            if (!await reader.ReadAsync(ct)) return PeopleResult.Unavailable;
            target = reader.GetFieldValue<uint>(0);
            role = reader.GetString(1);
        }
        await using (var update = connection.CreateCommand())
        {
            update.Transaction = tx;
            update.CommandText = "UPDATE mijn_role_invitations SET cancelled_at=UTC_TIMESTAMP(6) WHERE id=@id";
            Add(update, "@id", invitation);
            await update.ExecuteNonQueryAsync(ct);
        }
        await AuditAsync(connection, tx, "RoleInvitationCancelled", actor, group, target, role, ct);
        await tx.CommitAsync(ct);
        return PeopleResult.Done;
    }
}
