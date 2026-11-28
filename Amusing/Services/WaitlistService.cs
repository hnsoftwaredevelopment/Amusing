using System.Security.Claims;
using System.Net.Mail;
using Amusing.Facturatie;
using MySqlConnector;

namespace Amusing.Services;

public sealed record WaitlistChoir(uint Id,string Name,uint Headcount,string Stage,DateTime? Registered,string? Problem);
public sealed record WaitlistPlacement(string Name,DateTime Placed,DateTime Due,bool Queued,string? Problem);
public sealed record WaitlistOverview(DateTime Festival,DateTime Closing,List<WaitlistChoir> Choirs,List<WaitlistPlacement> Placements);
public sealed record WaitlistResult(int Placed,int Skipped,string? Error);

public sealed class WaitlistService(IConfiguration configuration,TimeProvider? timeProvider=null)
{
    public async Task<string?> ReturnToWaitlistAsync(ClaimsPrincipal user,uint festival,uint group)
    {
        if(user.Identity?.IsAuthenticated!=true || !uint.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier),out var actor))
            return "Log opnieuw in op Beheer.";
        await using var connection=Connection();await connection.OpenAsync();
        await using var tx=await connection.BeginTransactionAsync();
        await using(var auth=new MySqlCommand("SELECT user_id FROM ah_beheer WHERE user_id=@actor AND NULLIF(TRIM(role),'') IS NOT NULL AND COALESCE(NULLIF(PasswordHash,''),NULLIF(password,'')) IS NOT NULL",connection,tx))
        {
            auth.Parameters.AddWithValue("@actor",actor);
            if(await auth.ExecuteScalarAsync() is null)return "Je hebt geen toegang tot Beheer.";
        }
        // Match the placement worker's lock order before changing the registration.
        await using(var placement=new MySqlCommand("SELECT festival_id FROM mijn_wachtlijstplaatsingen WHERE festival_id=@festival AND zanggroep_id=@group FOR UPDATE",connection,tx))
        {
            placement.Parameters.AddWithValue("@festival",festival);placement.Parameters.AddWithValue("@group",group);
            await placement.ExecuteScalarAsync();
        }
        await using var update=new MySqlCommand("""
            UPDATE ah_inschrijvingen i JOIN ah_festivals f USING(festival_id)
            JOIN ah_zanggroepen g ON g.zanggroep_id=i.zanggroep_id SET i.wachtlijst=1
            WHERE i.festival_id=@festival AND i.zanggroep_id=@group AND COALESCE(i.wachtlijst,0)=0 AND DATE(f.festivaldatum)>=@today
                AND g.actief=1 AND (i.afgehaakt IS NULL OR CAST(i.afgehaakt AS CHAR) LIKE '0000-%');
            """,connection,tx);
        update.Parameters.AddWithValue("@festival",festival);update.Parameters.AddWithValue("@group",group);
        update.Parameters.AddWithValue("@today",Today.ToDateTime(TimeOnly.MinValue));
        if(await update.ExecuteNonQueryAsync()!=1)return "Op de wachtlijst zetten is niet mogelijk. Ververs de lijst en controleer of het koor actief is, niet is afgehaakt en de editie nog niet voorbij is.";
        await using var audit=new MySqlCommand("""
            INSERT INTO mijn_audit_log(occurred_at,actor_type,actor_id,action,group_id,festival_id,changed_fields)
            VALUES(UTC_TIMESTAMP(6),'beheer',@actor,'FestivalWaitlistRestored',@group,@festival,'WaitingList');
            """,connection,tx);
        audit.Parameters.AddWithValue("@festival",festival);audit.Parameters.AddWithValue("@group",group);audit.Parameters.AddWithValue("@actor",actor);
        await audit.ExecuteNonQueryAsync();await tx.CommitAsync();return null;
    }
    private MySqlConnection Connection() => new(configuration.GetConnectionString("DefaultConnection"));
    public DateOnly Today => DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc((timeProvider ?? TimeProvider.System).GetUtcNow().UtcDateTime,TimeZoneInfo.FindSystemTimeZoneById("Europe/Amsterdam")));

    public async Task<WaitlistOverview?> ReadAsync(uint festival)
    {
        await using var connection=Connection();
        await connection.OpenAsync();
        DateTime date,closing;
        await using(var cmd=new MySqlCommand("SELECT festivaldatum,DATE(eind_inschrijving) FROM ah_festivals WHERE festival_id=@festival",connection))
        {
            cmd.Parameters.AddWithValue("@festival",festival);
            await using var reader=await cmd.ExecuteReaderAsync();
            if(!await reader.ReadAsync()) return null;
            date=reader.GetDateTime(0); closing=reader.GetDateTime(1);
        }
        var choirs=new List<WaitlistChoir>();
        await using(var cmd=new MySqlCommand("""
            SELECT i.zanggroep_id,g.naam,i.aantal_deelnemers,i.podiumsoort,
                CASE WHEN CAST(i.ingeschreven AS CHAR) LIKE '0000-%' THEN NULL ELSE i.ingeschreven END
            FROM ah_inschrijvingen i JOIN ah_zanggroepen g USING(zanggroep_id)
            WHERE i.festival_id=@festival AND i.wachtlijst=1 AND g.actief=1
                AND (i.afgehaakt IS NULL OR CAST(i.afgehaakt AS CHAR) LIKE '0000-%')
            ORDER BY i.ingeschreven,g.naam
            """,connection))
        {
            cmd.Parameters.AddWithValue("@festival",festival);
            await using var reader=await cmd.ExecuteReaderAsync();
            while(await reader.ReadAsync()) choirs.Add(new(reader.GetUInt32(0),reader.GetString(1),reader.GetUInt32(2),reader.GetString(3),reader.IsDBNull(4)?null:reader.GetDateTime(4),null));
        }
        for(var index=0;index<choirs.Count;index++)
            choirs[index]=choirs[index] with { Problem=await ProblemAsync(connection,null,festival,choirs[index].Id) };
        var placements=new List<WaitlistPlacement>();
        await using(var cmd=new MySqlCommand("""
            SELECT g.naam,p.geplaatst_op,p.uiterste_betaaldatum,p.verwerkt_op IS NOT NULL AND p.foutmelding IS NULL,p.foutmelding
            FROM mijn_wachtlijstplaatsingen p JOIN ah_zanggroepen g USING(zanggroep_id)
            WHERE p.festival_id=@festival ORDER BY p.geplaatst_op DESC LIMIT 20
            """,connection))
        {
            cmd.Parameters.AddWithValue("@festival",festival);
            await using var reader=await cmd.ExecuteReaderAsync();
            while(await reader.ReadAsync()) placements.Add(new(reader.GetString(0),reader.GetDateTime(1),reader.GetDateTime(2),reader.GetBoolean(3),reader.IsDBNull(4)?null:reader.GetString(4)));
        }
        return new(date,closing,choirs,placements);
    }

    private static async Task<string?> ProblemAsync(MySqlConnection connection,MySqlTransaction? tx,uint festival,uint group)
    {
        await using(var cmd=new MySqlCommand("""
            SELECT p.email FROM ah_personen_rollen r JOIN ah_personen p USING(persoon_id)
            WHERE r.zanggroep_id=@group AND r.rol='contactpersoon1' AND p.actief=1
            """,connection,tx))
        {
            cmd.Parameters.AddWithValue("@group",group);
            await using var reader=await cmd.ExecuteReaderAsync();
            var reachable=false;
            while(await reader.ReadAsync()) reachable |= !reader.IsDBNull(0) && MailAddress.TryCreate(reader.GetString(0).Trim(),out _);
            if(!reachable) return "Vul eerst een geldig e-mailadres in bij contactpersoon 1.";
        }
        await using var tariff=new MySqlCommand($"""
            SELECT {FestivalTariefSql.Deelnemersprijs},p.prijs,f.btw_percentage
            FROM ah_inschrijvingen i JOIN ah_festivals f USING(festival_id)
            {FestivalTariefSql.PodiumtariefJoin}
            WHERE i.festival_id=@festival AND i.zanggroep_id=@group
            """,connection,tx);
        tariff.Parameters.AddWithValue("@festival",festival); tariff.Parameters.AddWithValue("@group",group);
        await using var rates=await tariff.ExecuteReaderAsync();
        return !await rates.ReadAsync() || rates.IsDBNull(0) || rates.IsDBNull(1) || rates.IsDBNull(2)
            ? "Deelnemerstarief, podiumprijs of btw ontbreekt voor deze inschrijving." : null;
    }

    public async Task<WaitlistResult> PlaceAsync(ClaimsPrincipal user,uint festival,IEnumerable<uint> selection,DateOnly? manualDue)
    {
        if(user.Identity?.IsAuthenticated != true || !uint.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier),out var actor))
            return new(0,0,"Log opnieuw in op Beheer.");
        var groups=selection.Distinct().OrderBy(id=>id).ToArray();
        if(groups.Length == 0) return new(0,0,"Selecteer minstens één koor.");
        await using var connection=Connection();
        await connection.OpenAsync();
        await using var tx=await connection.BeginTransactionAsync();
        await using(var admin=new MySqlCommand("SELECT user_id FROM ah_beheer WHERE user_id=@actor AND NULLIF(TRIM(role),'') IS NOT NULL AND COALESCE(NULLIF(PasswordHash,''),NULLIF(password,'')) IS NOT NULL",connection,tx))
        {
            admin.Parameters.AddWithValue("@actor",actor);
            if(await admin.ExecuteScalarAsync() is null) return new(0,0,"Je hebt geen toegang tot Beheer.");
        }
        DateOnly closing,date;
        await using(var read=new MySqlCommand("SELECT DATE(eind_inschrijving),festivaldatum FROM ah_festivals WHERE festival_id=@festival",connection,tx))
        {
            read.Parameters.AddWithValue("@festival",festival);
            await using var reader=await read.ExecuteReaderAsync();
            if(!await reader.ReadAsync()) return new(0,0,"De festivaleditie bestaat niet.");
            closing=DateOnly.FromDateTime(reader.GetDateTime(0)); date=DateOnly.FromDateTime(reader.GetDateTime(1));
        }
        var today=Today;
        if(date<today) return new(0,0,"Deze festivaleditie is voorbij.");
        if(today>closing && (manualDue is null || manualDue<today)) return new(0,0,"Kies een uiterste betaaldatum vanaf vandaag voor plaatsing na sluiting.");
        var due=today>closing ? manualDue!.Value : NotaBetaaltermijn.UitersteDatum(today,closing)!.Value;
        var placed=0; var skipped=0;
        foreach(var group in groups)
        {
            await using(var placementLock=new MySqlCommand("SELECT festival_id FROM mijn_wachtlijstplaatsingen WHERE festival_id=@festival AND zanggroep_id=@group FOR UPDATE",connection,tx))
            {
                placementLock.Parameters.AddWithValue("@festival",festival);placementLock.Parameters.AddWithValue("@group",group);
                await placementLock.ExecuteScalarAsync();
            }
            await using(var groupLock=new MySqlCommand("SELECT zanggroep_id FROM ah_zanggroepen WHERE zanggroep_id=@group FOR UPDATE",connection,tx))
            {
                groupLock.Parameters.AddWithValue("@group",group);
                await groupLock.ExecuteScalarAsync();
            }
            await using(var read=new MySqlCommand("""
                SELECT i.wachtlijst=1 AND g.actief=1 AND (i.afgehaakt IS NULL OR CAST(i.afgehaakt AS CHAR) LIKE '0000-%')
                FROM ah_inschrijvingen i JOIN ah_zanggroepen g USING(zanggroep_id)
                WHERE i.festival_id=@festival AND i.zanggroep_id=@group FOR UPDATE
                """,connection,tx))
            {
                read.Parameters.AddWithValue("@festival",festival); read.Parameters.AddWithValue("@group",group);
                if(Convert.ToInt32(await read.ExecuteScalarAsync()) != 1){skipped++;continue;}
            }
            var problem=await ProblemAsync(connection,tx,festival,group);
            if(problem is not null) return new(0,0,problem+" Ververs de wachtlijst en controleer de selectie.");
            await using var update=new MySqlCommand("""
                UPDATE ah_inschrijvingen SET wachtlijst=0 WHERE festival_id=@festival AND zanggroep_id=@group;
                INSERT INTO mijn_wachtlijstplaatsingen(festival_id,zanggroep_id,beheerder_id,geplaatst_op,betaalvanaf,uiterste_betaaldatum,volgende_poging)
                VALUES(@festival,@group,@actor,UTC_TIMESTAMP(6),@today,@due,UTC_TIMESTAMP(6))
                ON DUPLICATE KEY UPDATE beheerder_id=VALUES(beheerder_id),geplaatst_op=VALUES(geplaatst_op),
                    betaalvanaf=VALUES(betaalvanaf),uiterste_betaaldatum=VALUES(uiterste_betaaldatum),
                    volgende_poging=VALUES(volgende_poging),verwerkt_op=NULL,foutmelding=NULL;
                INSERT INTO mijn_audit_log(occurred_at,actor_type,actor_id,action,group_id,festival_id,changed_fields)
                VALUES(UTC_TIMESTAMP(6),'beheer',@actor,'FestivalWaitlistPlaced',@group,@festival,'WaitingList');
                """,connection,tx);
            update.Parameters.AddWithValue("@festival",festival); update.Parameters.AddWithValue("@group",group);
            update.Parameters.AddWithValue("@actor",actor); update.Parameters.AddWithValue("@today",today.ToDateTime(TimeOnly.MinValue));
            update.Parameters.AddWithValue("@due",due.ToDateTime(TimeOnly.MinValue));
            await update.ExecuteNonQueryAsync();
            placed++;
        }
        await tx.CommitAsync();
        return new(placed,skipped,null);
    }
}
