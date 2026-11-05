using System.ComponentModel.DataAnnotations;
namespace Amusing.Mijn.Services;
public sealed partial class ChoirPeople
{
    public async Task<PeopleResult> InviteAsync(uint actor,uint group,string email,string role,CancellationToken ct)
    {
        email=email.Trim();
        if(!Roles.ContainsKey(role) || email.Length>80 || !new EmailAddressAttribute().IsValid(email)) return PeopleResult.Invalid;
        await using var connection=await source.OpenConnectionAsync(ct);await using var tx=await connection.BeginTransactionAsync(ct);
        var roles=await LockAsync(connection,tx,group,ct);
        if(!Manages(roles,actor)) return PeopleResult.Forbidden;
        uint target;
        await using(var cmd=connection.CreateCommand())
        {
            cmd.Transaction=tx;cmd.CommandText="SELECT persoon_id FROM ah_personen WHERE email=@email AND actief=1 LIMIT 2";Add(cmd,"@email",email);
            await using var reader=await cmd.ExecuteReaderAsync(ct);
            if(!await reader.ReadAsync(ct)) return PeopleResult.Done;
            target=reader.GetFieldValue<uint>(0);
            if(await reader.ReadAsync(ct)) return PeopleResult.Done;
        }
        if(roles.Any(r=>r.Person==target && r.Role==role)) return PeopleResult.Done;
        await using(var cmd=connection.CreateCommand())
        {
            cmd.Transaction=tx;cmd.CommandText="SELECT id FROM mijn_role_invitations WHERE group_id=@group AND person_id=@target AND role_name=@role AND accepted_at IS NULL AND cancelled_at IS NULL AND expires_at>UTC_TIMESTAMP(6) LIMIT 1";
            Add(cmd,"@group",group);Add(cmd,"@target",target);Add(cmd,"@role",role);
            if(await cmd.ExecuteScalarAsync(ct) is not null) return PeopleResult.Done;
        }
        await using(var cmd=connection.CreateCommand())
        {
            cmd.Transaction=tx;cmd.CommandText="INSERT INTO mijn_role_invitations(id,group_id,person_id,role_name,created_by,created_at,expires_at) VALUES(@id,@group,@target,@role,@actor,UTC_TIMESTAMP(6),DATE_ADD(UTC_TIMESTAMP(6),INTERVAL 1 DAY))";
            Add(cmd,"@id",Guid.NewGuid().ToString("N"));Add(cmd,"@group",group);Add(cmd,"@target",target);Add(cmd,"@role",role);Add(cmd,"@actor",actor);await cmd.ExecuteNonQueryAsync(ct);
        }
        await AuditAsync(connection,tx,"RoleInvitationCreated",actor,group,target,role,ct);
        await tx.CommitAsync(ct);return PeopleResult.Done;
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
        string role;uint issuer;
        await using(var cmd=connection.CreateCommand())
        {
            cmd.Transaction=tx;cmd.CommandText="SELECT role_name,created_by,accepted_at,cancelled_at,expires_at>UTC_TIMESTAMP(6) FROM mijn_role_invitations WHERE id=@id AND person_id=@actor AND group_id=@group FOR UPDATE";
            Add(cmd,"@id",invitation);Add(cmd,"@actor",actor);Add(cmd,"@group",group);await using var reader=await cmd.ExecuteReaderAsync(ct);
            if(!await reader.ReadAsync(ct)) return PeopleResult.Unavailable;
            if(!reader.IsDBNull(2)) return PeopleResult.Done;
            if(!reader.IsDBNull(3) || !reader.GetBoolean(4)) return PeopleResult.Unavailable;
            role=reader.GetString(0);issuer=reader.GetFieldValue<uint>(1);
        }
        if(!Roles.ContainsKey(role) || !Manages(roles,issuer)) return PeopleResult.Unavailable;
        await using(var cmd=connection.CreateCommand())
        {
            cmd.Transaction=tx;cmd.CommandText="SELECT persoon_id FROM ah_personen WHERE persoon_id=@actor AND actief=1 FOR UPDATE";Add(cmd,"@actor",actor);
            if(await cmd.ExecuteScalarAsync(ct) is null) return PeopleResult.Unavailable;
        }
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
        await tx.CommitAsync(ct);return PeopleResult.Done;
    }
}
