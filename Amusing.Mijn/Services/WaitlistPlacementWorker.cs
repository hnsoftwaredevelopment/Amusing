using MySqlConnector;

namespace Amusing.Mijn.Services;

public sealed class WaitlistPlacementWorker(MySqlDataSource source,IFestivalClock clock,IConfiguration configuration,
    ILogger<WaitlistPlacementWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if(!configuration.GetValue<bool>("EnrollmentMail:Enabled")) return;
        var service=new ChoirEnrollments(source,clock,configuration:configuration);
        using var timer=new PeriodicTimer(TimeSpan.FromSeconds(5));
        do
        {
            try { await service.ProcessNextPlacementAsync(stoppingToken); }
            catch(Exception ex) when(!stoppingToken.IsCancellationRequested)
            {
                logger.LogWarning("Een wachtlijstplaatsing kon niet worden verwerkt ({ErrorType}). De verwerking wordt opnieuw geprobeerd.",ex.GetType().Name);
            }
        } while(await timer.WaitForNextTickAsync(stoppingToken));
    }
}

public sealed partial class ChoirEnrollments
{
    public async Task<bool> ProcessNextPlacementAsync(CancellationToken ct)
    {
        await using var connection=await source.OpenConnectionAsync(ct);
        await using var tx=await connection.BeginTransactionAsync(ct);
        uint festival,group,actor;
        await using(var cmd=Command(connection,tx,"""
            SELECT festival_id,zanggroep_id,beheerder_id FROM mijn_wachtlijstplaatsingen
            WHERE verwerkt_op IS NULL AND volgende_poging<=UTC_TIMESTAMP(6)
            ORDER BY geplaatst_op,festival_id,zanggroep_id LIMIT 1 FOR UPDATE SKIP LOCKED
            """))
        {
            await using var reader=await cmd.ExecuteReaderAsync(ct);
            if(!await reader.ReadAsync(ct)) return false;
            festival=reader.GetFieldValue<uint>(0); group=reader.GetFieldValue<uint>(1); actor=reader.GetFieldValue<uint>(2);
        }
        try
        {
            await RolesAsync(connection,tx,group,true,ct);
            var choir=await GroupAsync(connection,tx,group,true,ct);
            var registration=await RegistrationAsync(connection,tx,festival,group,ct,locked:true);
            if(choir is not { Active:true } || registration is null or { Cancelled:true } or { Waitlisted:true })
            {
                await using var cancelled=Command(connection,tx,"""
                    UPDATE mijn_wachtlijstplaatsingen SET verwerkt_op=UTC_TIMESTAMP(6),
                        foutmelding='Geen berichten klaargezet: de inschrijving is verwijderd, afgemeld of staat weer op de wachtlijst, of het koor is inactief.'
                    WHERE festival_id=@festival AND zanggroep_id=@group
                    """);
                Add(cancelled,"@festival",festival); Add(cancelled,"@group",group);
                await cancelled.ExecuteNonQueryAsync(ct);
                await tx.CommitAsync(ct);
                return true;
            }
            FestivalEdition edition;
            await using(var cmd=Command(connection,tx,"SELECT festival_id,festivaldatum,start_inschrijving,eind_inschrijving,wachtlijst FROM ah_festivals WHERE festival_id=@festival"))
            {
                Add(cmd,"@festival",festival);
                await using var reader=await cmd.ExecuteReaderAsync(ct);
                if(!await reader.ReadAsync(ct)) throw new InvalidOperationException("De festivaleditie bestaat niet.");
                edition=new(reader.GetFieldValue<uint>(0),DateOnly.FromDateTime(reader.GetDateTime(1)),DateOnly.FromDateTime(reader.GetDateTime(2)),DateOnly.FromDateTime(reader.GetDateTime(3)),reader.GetBoolean(4));
            }
            await QueueEnrollmentMailAsync(connection,tx,edition,group,choir.Name,registration,ct,placement:true,placingActor:new(actor,SupportMode.ActorType));
            await using(var processed=Command(connection,tx,"UPDATE mijn_wachtlijstplaatsingen SET verwerkt_op=UTC_TIMESTAMP(6),foutmelding=NULL WHERE festival_id=@festival AND zanggroep_id=@group"))
            {
                Add(processed,"@festival",festival); Add(processed,"@group",group);
                await processed.ExecuteNonQueryAsync(ct);
            }
            await tx.CommitAsync(ct);
            return true;
        }
        catch
        {
            await tx.RollbackAsync(CancellationToken.None);
            await using var retry=Command(connection,null,"""
                UPDATE mijn_wachtlijstplaatsingen SET volgende_poging=DATE_ADD(UTC_TIMESTAMP(6),INTERVAL 1 MINUTE),
                    foutmelding='Controleer contactpersonen, tarieven en de inschrijving. De verwerking wordt opnieuw geprobeerd.'
                WHERE festival_id=@festival AND zanggroep_id=@group AND verwerkt_op IS NULL
                """);
            Add(retry,"@festival",festival); Add(retry,"@group",group);
            await retry.ExecuteNonQueryAsync(CancellationToken.None);
            throw;
        }
    }
}
