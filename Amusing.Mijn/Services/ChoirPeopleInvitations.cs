using System.ComponentModel.DataAnnotations;
namespace Amusing.Mijn.Services;
public sealed partial class ChoirPeople
{
    public async Task<PeopleResult> InviteAsync(uint actor,uint group,string email,string role,CancellationToken ct)
    {
        email=email.Trim();
        if((!Roles.ContainsKey(role) && role != FirstContactTransfer) || email.Length>80 || !new EmailAddressAttribute().IsValid(email)) return PeopleResult.Invalid;
        await using var connection=await source.OpenConnectionAsync(ct);await using var tx=await connection.BeginTransactionAsync(ct);
        var roles=await LockAsync(connection,tx,group,ct);
        if(!SupportMode.CanSupport(supportContext,actor,group) && !Manages(roles,actor)) return PeopleResult.Forbidden;
        uint target;
        await using(var cmd=connection.CreateCommand())
        {
            cmd.Transaction=tx;cmd.CommandText="SELECT p.persoon_id FROM ah_personen p JOIN ah_personen_wachtwoorden w ON w.id=p.persoon_id WHERE p.email=@email AND p.actief=1 AND NULLIF(w.hash,'') IS NOT NULL LIMIT 2";Add(cmd,"@email",email);
            await using var reader=await cmd.ExecuteReaderAsync(ct);
            if(!await reader.ReadAsync(ct)) return PeopleResult.UnknownAccount;
            target=reader.GetFieldValue<uint>(0);
            if(await reader.ReadAsync(ct)) return PeopleResult.UnknownAccount;
        }
        if (role == FirstContactTransfer && roles.Any(r => r.Person == target && r.Role == "contactpersoon1")) return PeopleResult.Invalid;
        if(roles.Any(r=>r.Person==target && r.Role==role)) return PeopleResult.Done;
        if (await AtChoirLimitAsync(connection,tx,target,group,role,ct)) return PeopleResult.ChoirLimit;
        if (Exclusive(role) && roles.Any(r => r.Role == role)) return PeopleResult.RoleOccupied;
        if (Exclusive(role) || role == FirstContactTransfer)
        {
            await using var pending = connection.CreateCommand();
            pending.Transaction = tx;
            pending.CommandText = "SELECT COUNT(*) FROM mijn_role_invitations WHERE group_id=@group AND role_name=@role AND person_id<>@target AND accepted_at IS NULL AND cancelled_at IS NULL AND expires_at>UTC_TIMESTAMP(6)";
            Add(pending,"@group",group); Add(pending,"@role",role); Add(pending,"@target",target);
            if (Convert.ToInt32(await pending.ExecuteScalarAsync(ct)) > 0) return PeopleResult.RoleOccupied;
        }
        await using(var cmd=connection.CreateCommand())
        {
            cmd.Transaction=tx;cmd.CommandText="SELECT id FROM mijn_role_invitations WHERE group_id=@group AND person_id=@target AND role_name=@role AND accepted_at IS NULL AND cancelled_at IS NULL AND expires_at>UTC_TIMESTAMP(6) LIMIT 1";
            Add(cmd,"@group",group);Add(cmd,"@target",target);Add(cmd,"@role",role);
            if(await cmd.ExecuteScalarAsync(ct) is not null) return PeopleResult.Done;
        }
        await using(var cmd=connection.CreateCommand())
        {
            cmd.Transaction=tx;cmd.CommandText="INSERT INTO mijn_role_invitations(id,group_id,person_id,role_name,created_by,created_by_type,created_at,expires_at) VALUES(@id,@group,@target,@role,@actor,@actorType,UTC_TIMESTAMP(6),DATE_ADD(UTC_TIMESTAMP(6),INTERVAL 1 DAY))";
            Add(cmd,"@id",Guid.NewGuid().ToString("N"));Add(cmd,"@group",group);Add(cmd,"@target",target);Add(cmd,"@role",role);Add(cmd,"@actor",actor);Add(cmd,"@actorType",SupportMode.AuditType(supportContext,actor,group));await cmd.ExecuteNonQueryAsync(ct);
        }
        await AuditAsync(connection,tx,"RoleInvitationCreated",actor,group,target,role,ct);
        await tx.CommitAsync(ct);
        if (configuration?.GetValue<bool>("InvitationMail:Enabled") == true)
        {
            try
            {
                var origin = new Uri(configuration["AccountRegistration:Origin"]!, UriKind.Absolute);
                if (origin.Scheme != Uri.UriSchemeHttps) throw new InvalidOperationException("Invitation origin must use HTTPS.");
                var link = new Uri(origin, "/account/uitnodigingen");
                var sender = MailActor.From(supportContext?.HttpContext?.User);
                var testRecipient = configuration.GetValue<bool>("Mijn:IsTestEnvironment") && sender is not null
                    ? await sender.AddressAsync(source, ct) : null;
                await MijnMail.DeliverAsync(configuration, email, "Je hebt een uitnodiging voor Mijn Amusing",
                    $"Er staat een uitnodiging voor je klaar. Log in om die te bekijken: {link}", ct, testRecipient);
            }
            catch (Exception ex) when (!ct.IsCancellationRequested)
            {
                logger?.LogWarning("Uitnodigingsmail voor koor {GroupId} kon niet worden bezorgd ({ErrorType}).", group, ex.GetType().Name);
            }
        }
        return PeopleResult.Done;
    }
    public async Task<int> UnreadCountAsync(uint actor, CancellationToken ct)
    {
        await using var cmd = source.CreateCommand("""
            SELECT COUNT(*) FROM mijn_role_invitations WHERE person_id=@actor AND gezien_at IS NULL
                AND accepted_at IS NULL AND cancelled_at IS NULL AND expires_at>UTC_TIMESTAMP(6)
            """);
        Add(cmd, "@actor", actor);
        return Convert.ToInt32(await cmd.ExecuteScalarAsync(ct));
    }

    public async Task MarkSeenAsync(uint actor, IEnumerable<RoleInvitation> invitations, CancellationToken ct)
    {
        await using var connection = await source.OpenConnectionAsync(ct);
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = "UPDATE mijn_role_invitations SET gezien_at=UTC_TIMESTAMP(6) WHERE person_id=@actor AND id=@id AND gezien_at IS NULL";
        Add(cmd, "@actor", actor);
        Add(cmd, "@id", "");
        foreach (var invitation in invitations)
        {
            cmd.Parameters["@id"].Value = invitation.Id;
            await cmd.ExecuteNonQueryAsync(ct);
        }
    }

    public async Task<List<RoleInvitation>> IncomingAsync(uint actor,CancellationToken ct)
    {
        await using var cmd=source.CreateCommand("SELECT i.id,i.group_id,g.naam,i.role_name,i.expires_at FROM mijn_role_invitations i JOIN ah_zanggroepen g ON g.zanggroep_id=i.group_id WHERE i.person_id=@actor AND i.accepted_at IS NULL AND i.cancelled_at IS NULL AND i.expires_at>UTC_TIMESTAMP(6) ORDER BY i.created_at DESC");
        Add(cmd,"@actor",actor);await using var reader=await cmd.ExecuteReaderAsync(ct);var list=new List<RoleInvitation>();
        while(await reader.ReadAsync(ct)) list.Add(new(reader.GetString(0),reader.GetFieldValue<uint>(1),reader.GetString(2),reader.GetString(3),DateTime.SpecifyKind(reader.GetDateTime(4),DateTimeKind.Utc)));
        return list;
    }
    public async Task<PeopleResult> AcceptAsync(uint actor,string invitation,CancellationToken ct)
    {
        if(!Guid.TryParseExact(invitation,"N",out _)) return PeopleResult.Unavailable;
        uint group;
        await using(var cmd=source.CreateCommand("SELECT group_id FROM mijn_role_invitations WHERE id=@id AND person_id=@actor"))
        {
            Add(cmd,"@id",invitation);Add(cmd,"@actor",actor);
            var value=await cmd.ExecuteScalarAsync(ct);if(value is null) return PeopleResult.Unavailable;group=Convert.ToUInt32(value);
        }
        await using var connection=await source.OpenConnectionAsync(ct);await using var tx=await connection.BeginTransactionAsync(ct);
        var roles=await LockAsync(connection,tx,group,ct);
        string role, issuerType;uint issuer;
        await using(var cmd=connection.CreateCommand())
        {
            cmd.Transaction=tx;cmd.CommandText="SELECT role_name,created_by,accepted_at,cancelled_at,expires_at>UTC_TIMESTAMP(6),created_by_type FROM mijn_role_invitations WHERE id=@id AND person_id=@actor AND group_id=@group FOR UPDATE";
            Add(cmd,"@id",invitation);Add(cmd,"@actor",actor);Add(cmd,"@group",group);await using var reader=await cmd.ExecuteReaderAsync(ct);
            if(!await reader.ReadAsync(ct)) return PeopleResult.Unavailable;
            if(!reader.IsDBNull(2)) return PeopleResult.Done;
            if(!reader.IsDBNull(3) || !reader.GetBoolean(4)) return PeopleResult.Unavailable;
            role=reader.GetString(0);issuer=reader.GetFieldValue<uint>(1);issuerType=reader.GetString(5);
        }
        var transfer = role == FirstContactTransfer;
        if (!Roles.ContainsKey(role) && !transfer) return PeopleResult.Unavailable;
        if (issuerType == SupportMode.ActorType)
        {
            await using var issuerCheck = connection.CreateCommand();
            issuerCheck.Transaction = tx;
            issuerCheck.CommandText = "SELECT EXISTS(SELECT 1 FROM ah_beheer WHERE user_id=@issuer AND NULLIF(TRIM(role),'') IS NOT NULL AND COALESCE(NULLIF(PasswordHash,''),NULLIF(password,'')) IS NOT NULL)";
            Add(issuerCheck, "@issuer", issuer);
            if (Convert.ToInt32(await issuerCheck.ExecuteScalarAsync(ct)) != 1) return PeopleResult.Unavailable;
        }
        else if (!Manages(roles,issuer)) return PeopleResult.Unavailable;
        await using(var cmd=connection.CreateCommand())
        {
            cmd.Transaction=tx;cmd.CommandText="SELECT p.persoon_id FROM ah_personen p JOIN ah_personen_wachtwoorden w ON w.id=p.persoon_id WHERE p.persoon_id=@actor AND p.actief=1 AND NULLIF(w.hash,'') IS NOT NULL FOR UPDATE";Add(cmd,"@actor",actor);
            if(await cmd.ExecuteScalarAsync(ct) is null) return PeopleResult.Unavailable;
        }
        if (transfer)
        {
            role = "contactpersoon1";
            await using var remove = connection.CreateCommand();
            remove.Transaction = tx;
            remove.CommandText = "DELETE FROM ah_personen_rollen WHERE zanggroep_id=@group AND rol='contactpersoon1' AND persoon_id<>@actor";
            Add(remove,"@group",group); Add(remove,"@actor",actor);
            await remove.ExecuteNonQueryAsync(ct);
            await AuditAsync(connection,tx,"FirstContactTransferred",actor,group,issuer,role,ct);
        }
        else if (Exclusive(role) && roles.Any(r => r.Role == role && r.Person != actor)) return PeopleResult.RoleOccupied;
        if (await AtChoirLimitAsync(connection,tx,actor,group,role,ct)) return PeopleResult.ChoirLimit;
        if(!roles.Any(r=>r.Person==actor && r.Role==role))
        {
            await using var cmd=connection.CreateCommand();cmd.Transaction=tx;cmd.CommandText="INSERT INTO ah_personen_rollen(persoon_id,zanggroep_id,rol) VALUES(@actor,@group,@role)";
            Add(cmd,"@actor",actor);Add(cmd,"@group",group);Add(cmd,"@role",role);await cmd.ExecuteNonQueryAsync(ct);
        }
        await using(var cmd=connection.CreateCommand())
        {
            cmd.Transaction=tx;cmd.CommandText="UPDATE mijn_role_invitations SET accepted_at=UTC_TIMESTAMP(6) WHERE id=@id";Add(cmd,"@id",invitation);await cmd.ExecuteNonQueryAsync(ct);
        }
        await AuditAsync(connection,tx,"RoleInvitationAccepted",actor,group,actor,role,ct);
        await tx.CommitAsync(ct);
        return PeopleResult.Done;
    }
}
