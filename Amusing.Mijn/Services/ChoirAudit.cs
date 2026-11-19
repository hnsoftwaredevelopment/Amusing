using MySqlConnector;

namespace Amusing.Mijn.Services;

public sealed record ChoirAuditEntry(ulong Id, DateTime TimeUtc, string By, string Action, string Fields, string ActorType);
public sealed record ChoirAuditPage(string ChoirName, IReadOnlyList<ChoirAuditEntry> Entries, ulong? Next);

public sealed class ChoirAudit(MySqlDataSource source, IHttpContextAccessor context)
{
    public async Task<ChoirAuditPage?> ReadAsync(uint actor, uint group, ulong? before, CancellationToken ct)
    {
        const int pageSize = 50;
        var support = SupportMode.CanSupport(context, actor, group);
        await using var connection = await source.OpenConnectionAsync(ct);
        await using var command = new MySqlCommand("""
            SELECT g.naam, l.id, l.occurred_at,
                CASE WHEN l.actor_type='beheer' THEN COALESCE(TRIM(b.weergavenaam), '')
                    ELSE COALESCE(NULLIF(TRIM(CONCAT_WS(' ', p.voornaam, p.tussenvoegsel, p.achternaam)), ''), '') END,
                l.action, COALESCE(l.changed_fields,l.role_name,''), l.actor_type
            FROM ah_zanggroepen g
            LEFT JOIN mijn_audit_log l ON l.group_id=g.zanggroep_id
                AND l.actor_type IN ('beheer','choir_person','system')
                AND l.action IN ('ChoirCreated','ChoirProfileChanged','ChoirImageChanged','ChoirImageRemoved',
                    'FestivalEnrolled','FestivalEnrollmentUpdated','FestivalEnrollmentConfirmed','FestivalWaitlistPlaced','ParticipationNoteSent','ParticipationNoteCopySent',
                    'RoleInvitationCreated','RoleInvitationAccepted','RoleInvitationCancelled','FirstContactTransferred','RoleRemoved')
                AND (@before IS NULL OR l.id < @before)
            LEFT JOIN ah_beheer b ON l.actor_type='beheer' AND b.user_id=l.actor_id
            LEFT JOIN ah_personen p ON l.actor_type='choir_person' AND p.persoon_id=l.actor_id
            WHERE g.zanggroep_id=@group AND (@support OR EXISTS (
                SELECT 1 FROM ah_personen_rollen r JOIN ah_personen a USING (persoon_id)
                WHERE r.zanggroep_id=g.zanggroep_id AND r.persoon_id=@actor AND a.actief=1
            ))
            ORDER BY l.id DESC LIMIT 51
            """, connection);
        command.Parameters.AddWithValue("@group", group);
        command.Parameters.AddWithValue("@actor", actor);
        command.Parameters.AddWithValue("@support", support);
        command.Parameters.AddWithValue("@before", (object?)before ?? DBNull.Value);

        await using var reader = await command.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct)) return null;

        var name = reader.GetString(0);
        var entries = new List<ChoirAuditEntry>();
        do
        {
            if (reader.IsDBNull(1)) break;
            entries.Add(new(reader.GetUInt64(1), DateTime.SpecifyKind(reader.GetDateTime(2), DateTimeKind.Utc),
                reader.GetString(3), reader.GetString(4), reader.GetString(5), reader.GetString(6)));
        } while (await reader.ReadAsync(ct));

        ulong? next = null;
        if (entries.Count > pageSize)
        {
            entries.RemoveAt(pageSize);
            next = entries[^1].Id;
        }
        return new(name, entries, next);
    }
}
