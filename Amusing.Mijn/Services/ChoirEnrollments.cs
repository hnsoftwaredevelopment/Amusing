using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using MySqlConnector;
namespace Amusing.Mijn.Services;
public sealed partial class ChoirEnrollments(MySqlDataSource source,IFestivalClock clock,
    IHttpContextAccessor? supportContext = null, IConfiguration? configuration = null)
{
    private sealed record Role(uint Person,string Name,bool Active,string Phone,string Mobile);
    private sealed record Group(string Name,string City,string Country,int Genre,string Iban,bool Active);
    private static void Add(DbCommand cmd,string name,object value){var p=cmd.CreateParameter();p.ParameterName=name;p.Value=value;cmd.Parameters.Add(p);}
    private static DbCommand Command(DbConnection connection,DbTransaction? tx,string sql)
    {
        var cmd=connection.CreateCommand();cmd.Transaction=tx;cmd.CommandText=sql;return cmd;
    }
    private static async Task<List<Role>> RolesAsync(DbConnection connection,DbTransaction? tx,uint group,bool locked,CancellationToken ct)
    {
        await using var cmd=Command(connection,tx,"SELECT r.persoon_id,r.rol,p.actief,COALESCE(c.telefoon_vast,''),COALESCE(c.telefoon_mobiel,'') FROM ah_personen_rollen r JOIN ah_personen p ON p.persoon_id=r.persoon_id LEFT JOIN ah_contactgegevens c ON c.persoon_id=p.persoon_id WHERE r.zanggroep_id=@group ORDER BY r.persoon_id,r.rol"+(locked?" FOR UPDATE":""));
        Add(cmd,"@group",group);await using var reader=await cmd.ExecuteReaderAsync(ct);var roles=new List<Role>();
        while(await reader.ReadAsync(ct)) roles.Add(new(reader.GetFieldValue<uint>(0),reader.GetString(1),!reader.IsDBNull(2)&&reader.GetBoolean(2),reader.GetString(3),reader.GetString(4)));
        return roles;
    }
    private static async Task<Group?> GroupAsync(DbConnection connection,DbTransaction? tx,uint group,bool locked,CancellationToken ct)
    {
        await using var cmd=Command(connection,tx,"SELECT naam,standplaats,land,genre_id,rekeningnr,actief FROM ah_zanggroepen WHERE zanggroep_id=@group"+(locked?" FOR UPDATE":""));Add(cmd,"@group",group);
        await using var reader=await cmd.ExecuteReaderAsync(ct);
        if(!await reader.ReadAsync(ct)) return null;
        string Text(int i)=>reader.IsDBNull(i)?"":reader.GetString(i);
        return new(Text(0),Text(1),Text(2),reader.IsDBNull(3)?0:Convert.ToInt32(reader.GetValue(3)),Text(4),!reader.IsDBNull(5)&&reader.GetBoolean(5));
    }
    private static async Task<FestivalEdition?> FestivalAsync(DbConnection connection,DbTransaction? tx,DateOnly today,bool locked,CancellationToken ct)
    {
        await using var cmd=Command(connection,tx,"SELECT festival_id,festivaldatum,start_inschrijving,eind_inschrijving,wachtlijst,eerste_optreden,laatste_optreden FROM ah_festivals WHERE festivaldatum>=@today ORDER BY festivaldatum,festival_id LIMIT 1"+(locked?" FOR UPDATE":""));
        Add(cmd,"@today",today.ToDateTime(TimeOnly.MinValue));await using var reader=await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct)?new FestivalEdition(reader.GetFieldValue<uint>(0),DateOnly.FromDateTime(reader.GetDateTime(1)),DateOnly.FromDateTime(reader.GetDateTime(2)),DateOnly.FromDateTime(reader.GetDateTime(3)),reader.GetBoolean(4)) { FirstPerformance = reader.GetFieldValue<TimeSpan>(5), LastPerformance = reader.GetFieldValue<TimeSpan>(6) }:null;
    }
    private static async Task<List<EnrollmentStage>> StagesAsync(DbConnection connection,DbTransaction? tx,uint festival,CancellationToken ct)
    {
        await using var cmd=Command(connection,tx,"SELECT type,beschrijving FROM ah_podia_typen WHERE versie=(SELECT MAX(versie) FROM ah_podia_typen WHERE versie<=@festival) ORDER BY type");
        Add(cmd,"@festival",festival);await using var reader=await cmd.ExecuteReaderAsync(ct);var stages=new List<EnrollmentStage>();
        while(await reader.ReadAsync(ct)) stages.Add(new(reader.GetString(0),reader.IsDBNull(1)?"":reader.GetString(1)));
        return stages;
    }
    private static async Task<FestivalRegistration?> RegistrationAsync(DbConnection connection,DbTransaction? tx,uint festival,uint group,CancellationToken ct,bool locked=false)
    {
        await using var cmd=Command(connection,tx,"""
            SELECT aantal_deelnemers,podiumsoort,wens_4,binnenoptredens,buitenoptredens,wachtlijst,
                   COALESCE(CAST(afgehaakt AS CHAR) NOT LIKE '0000-%',0),
                   COALESCE(CAST(betaald AS CHAR) NOT LIKE '0000-%',0),
                   COALESCE(CAST(bevestigd AS CHAR) >= '2000-01-01',0),
                   podiumkeuze_geforceerd,beschikbaar_van,beschikbaar_tot
            FROM ah_inschrijvingen WHERE festival_id=@festival AND zanggroep_id=@group
            """+(locked?" FOR UPDATE":""));
        Add(cmd,"@festival",festival);Add(cmd,"@group",group);await using var reader=await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct)?new FestivalRegistration(reader.GetFieldValue<uint>(0),reader.GetString(1),reader.GetString(2)=="ja",Convert.ToInt32(reader.GetValue(3)),Convert.ToInt32(reader.GetValue(4)),reader.GetBoolean(5),reader.GetBoolean(6),reader.GetBoolean(7),reader.GetBoolean(8),reader.GetBoolean(9)) { AvailableFrom = reader.GetFieldValue<TimeSpan>(10), AvailableUntil = reader.GetFieldValue<TimeSpan>(11) }:null;
    }
    private static bool Contact(List<Role> roles,uint actor)=>roles.Any(r=>r.Person==actor && r.Active && r.Name is "contactpersoon1" or "contactpersoon2");
    private static List<string> Missing(Group group,List<Role> roles)
    {
        var missing=new List<string>();
        if(!group.Active) missing.Add("Dit koor is niet actief. Neem contact op met de organisatie.");
        if(string.IsNullOrWhiteSpace(group.Name)||string.IsNullOrWhiteSpace(group.City)||string.IsNullOrWhiteSpace(group.Country)||group.Genre<=0) missing.Add("Vul de naam, standplaats, het land en genre in bij het koorprofiel.");
        var iban=string.Concat(group.Iban.Where(c=>!char.IsWhiteSpace(c))).ToUpperInvariant();
        if(!ChoirProfileInput.IsIban(iban)) missing.Add("Vul een geldig IBAN in bij het koorprofiel.");
        if(!roles.Any(r=>r.Active&&r.Name=="dirigent")) missing.Add("Koppel een actieve dirigent via Personen en rollen.");
        var primary=roles.Where(r=>r.Active&&r.Name=="contactpersoon1").ToList();
        if(primary.Count==0) missing.Add("Koppel minstens één actieve eerste contactpersoon.");
        if(primary.Any(r=>!PersonalContactInput.PhoneValid(r.Phone)&&!PersonalContactInput.PhoneValid(r.Mobile))) missing.Add("Iedere eerste contactpersoon moet een geldig vast of mobiel telefoonnummer hebben.");
        return missing;
    }
    public async Task<EnrollmentView?> ViewAsync(uint actor,uint group,CancellationToken ct)
    {
        await using var connection=await source.OpenConnectionAsync(ct);
        var roles=await RolesAsync(connection,null,group,false,ct);
        if(!SupportMode.CanSupport(supportContext,actor,group) && !roles.Any(r=>r.Person==actor&&r.Active)) return null;
        var info=await GroupAsync(connection,null,group,false,ct);if(info is null) return null;
        var today=DateOnly.FromDateTime(clock.Now);var festival=await FestivalAsync(connection,null,today,false,ct);
        var stages=festival is null?[]:await StagesAsync(connection,null,festival.Id,ct);
        var registration=festival is null?null:await RegistrationAsync(connection,null,festival.Id,group,ct);
        return new(info.Name,SupportMode.CanSupport(supportContext,actor,group) || Contact(roles,actor),festival,stages,registration,Missing(info,roles),festival?.IsOpen(today)==true)
        {
            AfterClosing = festival is not null && today > festival.End,
            ParticipationAmount = festival is not null && registration is { Cancelled: false }
                ? await ParticipationAmountAsync(connection, festival.Id, registration, ct) : null,
            GroupActive = info.Active
        };
    }
    public async Task<EnrollmentResult> RegisterAsync(uint actor,uint group,EnrollmentInput input,CancellationToken ct)
    {
        await using var connection=await source.OpenConnectionAsync(ct);await using var tx=await connection.BeginTransactionAsync(ct);
        var roles=await RolesAsync(connection,tx,group,true,ct);
        if(!SupportMode.CanSupport(supportContext,actor,group) && !Contact(roles,actor)) return EnrollmentResult.Forbidden;
        var info=await GroupAsync(connection,tx,group,true,ct);if(info is null) return EnrollmentResult.Forbidden;
        var festival=await FestivalAsync(connection,tx,DateOnly.FromDateTime(clock.Now),true,ct);
        if(festival is null) return EnrollmentResult.NoFestival;
        if(input.FestivalId!=festival.Id) return EnrollmentResult.EditionChanged;
        if(await RegistrationAsync(connection,tx,festival.Id,group,ct) is not null) return EnrollmentResult.AlreadyRegistered;
        if(!festival.IsOpen(DateOnly.FromDateTime(clock.Now))) return EnrollmentResult.Closed;
        if(Missing(info,roles).Count>0) return EnrollmentResult.Incomplete;
        input.PodiumType=input.PodiumType?.Trim().ToUpperInvariant() ?? "";
        if(!Validator.TryValidateObject(input,new ValidationContext(input),new List<ValidationResult>(),true)) return EnrollmentResult.Invalid;
        if (!ResolveAvailability(festival, input.AvailableFrom, input.AvailableUntil, null, out var from, out var until)) return EnrollmentResult.Invalid;
        var stages=await StagesAsync(connection,tx,festival.Id,ct);
        if(!stages.Any(p=>p.Id==input.PodiumType)) return EnrollmentResult.Invalid;
        var now=clock.Now;var today=DateOnly.FromDateTime(now);
        if(festival.Date<today) return EnrollmentResult.EditionChanged;
        if(!festival.IsOpen(today)) return EnrollmentResult.Closed;
        await using(var cmd=Command(connection,tx,"INSERT INTO ah_inschrijvingen(festival_id,zanggroep_id,aantal_deelnemers,podiumsoort,wens_4,ingeschreven,wachtlijst,binnenoptredens,buitenoptredens,beschikbaar_van,beschikbaar_tot) VALUES(@festival,@group,@count,@stage,@review,@now,@waiting,@inside,@outside,@from,@until)"))
        {
            Add(cmd,"@festival",festival.Id);Add(cmd,"@group",group);Add(cmd,"@count",input.Headcount);Add(cmd,"@stage",input.PodiumType);Add(cmd,"@review",input.Review?"ja":"nee");Add(cmd,"@now",now);Add(cmd,"@waiting",festival.WaitingList);Add(cmd,"@inside",input.Preference[0]-'0');Add(cmd,"@outside",input.Preference[1]-'0');
            Add(cmd,"@from",from);Add(cmd,"@until",until);
            try{await cmd.ExecuteNonQueryAsync(ct);}catch(MySqlException error) when(error.Number==1062){return EnrollmentResult.AlreadyRegistered;}
        }
        await using(var cmd=Command(connection,tx,"INSERT INTO mijn_audit_log(occurred_at,actor_type,actor_id,action,group_id,festival_id,changed_fields) VALUES(UTC_TIMESTAMP(6),@actorType,@actor,'FestivalEnrolled',@group,@festival,'Registration,Headcount,PodiumType,Preference,Review,WaitingList,AvailableFrom,AvailableUntil')"))
        {
            Add(cmd,"@actorType",SupportMode.AuditType(supportContext,actor,group));Add(cmd,"@actor",actor);Add(cmd,"@group",group);Add(cmd,"@festival",festival.Id);await cmd.ExecuteNonQueryAsync(ct);
        }
        var registration = await RegistrationAsync(connection, tx, festival.Id, group, ct);
        await QueueEnrollmentMailAsync(connection, tx, festival, group, info.Name, registration!, ct);
        await tx.CommitAsync(ct);return EnrollmentResult.Saved;
    }
}
