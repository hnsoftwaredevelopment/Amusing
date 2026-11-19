using System.Data.Common;
using MySqlConnector;
namespace Amusing.Mijn.Services;
public enum PeopleResult { Done,Forbidden,Invalid,LastContact,Unavailable,UnknownAccount,RoleOccupied,ChoirLimit }
public sealed record ChoirPerson(uint Id,string Name,string? Email,string? Phone,string? Mobile,List<string> Roles);
public sealed record ChoirRoster(string Name,bool CanManage,List<ChoirPerson> People);
public sealed record RoleInvitation(string Id,uint GroupId,string GroupName,string Role,DateTime ExpiresAt);
public sealed record OutgoingRoleInvitation(string Id, string Name, string Role, DateTime ExpiresAt, bool AccountActive);
public sealed partial class ChoirPeople(MySqlDataSource source, IHttpContextAccessor? supportContext = null, IConfiguration? configuration = null, ILogger<ChoirPeople>? logger = null)
{
    public const string FirstContactTransfer = "overdracht_contactpersoon1";
    private static bool Exclusive(string role) => role is "contactpersoon1" or "contactpersoon2" or "penningmeester";
    private static async Task<bool> AtChoirLimitAsync(DbConnection connection, DbTransaction tx, uint person, uint group, string role, CancellationToken ct)
    {
        if (role is not ("muzikant" or "dirigent")) return false;
        await using (var personLock = connection.CreateCommand())
        {
            personLock.Transaction = tx;
            personLock.CommandText = "SELECT persoon_id FROM ah_personen WHERE persoon_id=@person FOR UPDATE";
            Add(personLock,"@person",person);
            await personLock.ExecuteScalarAsync(ct);
        }
        await using var count = connection.CreateCommand();
        count.Transaction = tx;
        count.CommandText = "SELECT zanggroep_id FROM ah_personen_rollen WHERE persoon_id=@person AND rol IN ('dirigent','muzikant') AND zanggroep_id<>@group FOR UPDATE";
        Add(count,"@person",person); Add(count,"@group",group);
        var groups = new HashSet<uint>();
        await using var reader = await count.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct)) groups.Add(reader.GetFieldValue<uint>(0));
        return groups.Count >= 2;
    }
    public static readonly IReadOnlyDictionary<string,string> Roles=new System.Collections.ObjectModel.ReadOnlyDictionary<string,string>(new Dictionary<string,string>
    {
        ["contactpersoon1"]="Eerste contactpersoon",["contactpersoon2"]="Tweede contactpersoon",
        ["dirigent"]="Dirigent",["penningmeester"]="Penningmeester",["muzikant"]="Muzikant",["zanger"]="Zanger",["vrijwilliger"]="Vrijwilliger"
    });
    private const string ContactCheck="EXISTS(SELECT 1 FROM ah_personen_rollen ar JOIN ah_personen ap ON ap.persoon_id=ar.persoon_id WHERE ar.zanggroep_id=@group AND ar.persoon_id=@actor AND ap.actief=1 AND ar.rol IN ('contactpersoon1','contactpersoon2'))";
    public async Task<ChoirRoster?> RosterAsync(uint actor,uint group,CancellationToken ct)
    {
        string name;bool manage;
        await using(var cmd=source.CreateCommand("SELECT g.naam,@support OR EXISTS(SELECT 1 FROM ah_personen_rollen ar JOIN ah_personen ap ON ap.persoon_id=ar.persoon_id WHERE ar.zanggroep_id=g.zanggroep_id AND ar.persoon_id=@actor AND ar.rol='contactpersoon1' AND ap.actief=1) FROM ah_zanggroepen g WHERE g.zanggroep_id=@group AND (@support OR EXISTS(SELECT 1 FROM ah_personen_rollen ar WHERE ar.zanggroep_id=g.zanggroep_id AND ar.persoon_id=@actor))"))
        {
            Add(cmd,"@actor",actor);Add(cmd,"@group",group);Add(cmd,"@support",SupportMode.CanSupport(supportContext,actor,group));await using var reader=await cmd.ExecuteReaderAsync(ct);
            if(!await reader.ReadAsync(ct)) return null;name=reader.GetString(0);manage=reader.GetBoolean(1);
        }
        var people=new Dictionary<uint,ChoirPerson>();
        await using(var cmd=source.CreateCommand($"""
            SELECT p.persoon_id,CONCAT_WS(' ',NULLIF(p.voornaam,''),NULLIF(p.tussenvoegsel,''),NULLIF(p.achternaam,'')),
            CASE WHEN @support OR {ContactCheck} THEN p.email END,
            CASE WHEN @support OR {ContactCheck} THEN c.telefoon_vast END,
            CASE WHEN @support OR {ContactCheck} THEN c.telefoon_mobiel END,r.rol
            FROM ah_personen_rollen r JOIN ah_personen p ON p.persoon_id=r.persoon_id
            LEFT JOIN ah_contactgegevens c ON c.persoon_id=p.persoon_id
            WHERE r.zanggroep_id=@group AND (@support OR EXISTS(SELECT 1 FROM ah_personen_rollen ar WHERE ar.zanggroep_id=@group AND ar.persoon_id=@actor))
            ORDER BY p.voornaam,p.achternaam,p.persoon_id,r.rol
            """))
        {
            Add(cmd,"@actor",actor);Add(cmd,"@group",group);Add(cmd,"@support",SupportMode.CanSupport(supportContext,actor,group));await using var reader=await cmd.ExecuteReaderAsync(ct);
            while(await reader.ReadAsync(ct))
            {
                var id=reader.GetFieldValue<uint>(0);
                string? Text(int i)=>reader.IsDBNull(i)?null:reader.GetString(i);
                if(!people.TryGetValue(id,out var person)) people[id]=person=new(id,reader.GetString(1),Text(2),Text(3),Text(4),[]);
                person.Roles.Add(reader.GetString(5));
            }
        }
        return new(name,manage,people.Values.ToList());
    }
    private sealed record LockedRole(uint Person,string Role,bool Active);
    private static bool Manages(List<LockedRole> roles,uint person)=>roles.Any(r=>r.Person==person && r.Role=="contactpersoon1" && r.Active);
    private static async Task<List<LockedRole>> LockAsync(DbConnection connection,DbTransaction tx,uint group,CancellationToken ct)
    {
        var roles=new List<LockedRole>();
        await using(var cmd=connection.CreateCommand())
        {
            cmd.Transaction=tx;cmd.CommandText="SELECT r.persoon_id,r.rol,p.actief FROM ah_personen_rollen r JOIN ah_personen p ON p.persoon_id=r.persoon_id WHERE r.zanggroep_id=@group ORDER BY r.persoon_id,r.rol FOR UPDATE";
            Add(cmd,"@group",group);await using var reader=await cmd.ExecuteReaderAsync(ct);
            while(await reader.ReadAsync(ct)) roles.Add(new(reader.GetFieldValue<uint>(0),reader.GetString(1),reader.GetBoolean(2)));
        }
        await using(var cmd=connection.CreateCommand())
        {
            cmd.Transaction=tx;cmd.CommandText="SELECT zanggroep_id FROM ah_zanggroepen WHERE zanggroep_id=@group FOR UPDATE";Add(cmd,"@group",group);
            if(await cmd.ExecuteScalarAsync(ct) is null) roles.Clear();
        }
        return roles;
    }
    public async Task<PeopleResult> RemoveAsync(uint actor,uint group,uint person,string role,CancellationToken ct)
    {
        if(!Roles.ContainsKey(role)) return PeopleResult.Invalid;
        await using var connection=await source.OpenConnectionAsync(ct);await using var tx=await connection.BeginTransactionAsync(ct);
        var roles=await LockAsync(connection,tx,group,ct);
        if(!SupportMode.CanSupport(supportContext,actor,group) && !Manages(roles,actor)) return PeopleResult.Forbidden;
        if(!roles.Any(r=>r.Person==person && r.Role==role)) return PeopleResult.Done;
        if(role=="contactpersoon1" && !roles.Any(r=>r.Person!=person && r.Role=="contactpersoon1" && r.Active)) return PeopleResult.LastContact;
        await using(var cmd=connection.CreateCommand())
        {
            cmd.Transaction=tx;cmd.CommandText="DELETE FROM ah_personen_rollen WHERE persoon_id=@person AND zanggroep_id=@group AND rol=@role";
            Add(cmd,"@person",person);Add(cmd,"@group",group);Add(cmd,"@role",role);await cmd.ExecuteNonQueryAsync(ct);
        }
        await AuditAsync(connection,tx,"RoleRemoved",actor,group,person,role,ct);
        await tx.CommitAsync(ct);return PeopleResult.Done;
    }
    private async Task AuditAsync(DbConnection connection,DbTransaction tx,string action,uint actor,uint group,uint target,string role,CancellationToken ct)
    {
        await using var cmd=connection.CreateCommand();cmd.Transaction=tx;
        cmd.CommandText="INSERT INTO mijn_audit_log(occurred_at,actor_type,actor_id,action,group_id,changed_fields,target_person_id,role_name) VALUES(UTC_TIMESTAMP(6),@actorType,@actor,@action,@group,'Roles',@target,@role)";
        Add(cmd,"@actor",actor);Add(cmd,"@actorType",SupportMode.AuditType(supportContext,actor,group));Add(cmd,"@action",action);Add(cmd,"@group",group);Add(cmd,"@target",target);Add(cmd,"@role",role);await cmd.ExecuteNonQueryAsync(ct);
    }
    private static void Add(DbCommand cmd,string name,object value){var p=cmd.CreateParameter();p.ParameterName=name;p.Value=value;cmd.Parameters.Add(p);}
}
