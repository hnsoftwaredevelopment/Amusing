extern alias beheer;
using System.Security.Claims;
using System.Text.Json;
using Amusing.Mijn.Services;
using Microsoft.Extensions.Configuration;
using WaitlistService=beheer::Amusing.Services.WaitlistService;

namespace Amusing.Mijn.Tests;

public partial class LoginTests
{
    [Theory]
    [InlineData("nl","Koor vanaf wachtlijst geplaatst")]
    [InlineData("de","Chor von der Warteliste zugelassen")]
    [InlineData("en","Choir placed from the waiting list")]
    public void PlacementLogHasTranslatedDescription(string language,string expected) =>
        Assert.Equal(expected,UiText.Get("KoorVanafWachtlijstGeplaatst",language));

    private static ClaimsPrincipal PlacementAdmin() => new(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier,"42") },"test"));
    private sealed class PlacementTimeProvider(FixedFestivalClock clock) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new DateTimeOffset(clock.Now,TimeSpan.FromHours(1)).ToUniversalTime();
    }
    private WaitlistService PlacementService(FixedFestivalClock clock) => new(new ConfigurationBuilder().AddInMemoryCollection(
        new Dictionary<string,string?> { ["ConnectionStrings:DefaultConnection"]=Environment.GetEnvironmentVariable("MIJN_TEST_CONNECTION") }).Build(),new PlacementTimeProvider(clock));

    private async Task<ChoirEnrollments> ReadyWaitlistedChoir(FixedFestivalClock clock)
    {
        await ReadyChoir();
        await Execute("INSERT INTO ah_beheer(user_id,username,role,PasswordHash) VALUES(42,'plaatsingsbeheer','admin',@hash); UPDATE ah_festivals SET wachtlijst=1 WHERE festival_id=2027",originalHash);
        var service=new ChoirEnrollments(source,clock);
        Assert.Equal(EnrollmentResult.Saved,await service.RegisterAsync(42,1,ValidEnrollment(),default));
        await Execute("UPDATE mijn_inschrijfmail SET verzonden=UTC_TIMESTAMP(6)");
        return service;
    }

    [DatabaseFact]
    public async Task PlacementQueuesOneLocalizedMessageAndInvoiceAndIgnoresRepeatedPlacement()
    {
        var clock=new FixedFestivalClock();
        var mail=await ReadyWaitlistedChoir(clock);
        await Execute("UPDATE ah_personen SET actief=1 WHERE persoon_id=43; INSERT INTO ah_personen_rollen VALUES(43,1,'penningmeester'); INSERT INTO mijn_taalvoorkeur VALUES(42,'de'),(43,'en')");
        clock.Now=clock.Now.AddDays(7);
        var service=PlacementService(clock);
        var result=await service.PlaceAsync(PlacementAdmin(),2027,new uint[]{1},null);
        Assert.Null(result.Error); Assert.Equal(1,result.Placed);
        Assert.False((await mail.ViewAsync(42,1,default))!.Registration!.Waitlisted);
        var repeated=await service.PlaceAsync(PlacementAdmin(),2027,new uint[]{1},null);
        Assert.Equal(0,repeated.Placed); Assert.Equal(1,repeated.Skipped);
        var processing=await Task.WhenAll(mail.ProcessNextPlacementAsync(default),mail.ProcessNextPlacementAsync(default));
        Assert.Single(processing,value=>value);
        await using(var read=source.CreateCommand("SELECT soort,onderwerp,nota_gegevens,aanvrager_type,aanvrager_id FROM mijn_inschrijfmail WHERE verzonden IS NULL ORDER BY soort"))
        await using(var reader=await read.ExecuteReaderAsync())
        {
            Assert.True(await reader.ReadAsync());
            Assert.Equal("koor",reader.GetString(0));
            Assert.StartsWith("Teilnahme bestätigt Amusing",reader.GetString(1));
            Assert.Equal("beheer",reader.GetString(3)); Assert.Equal(42u,reader.GetFieldValue<uint>(4));
            Assert.True(await reader.ReadAsync());
            Assert.Equal("nota",reader.GetString(0));
            Assert.StartsWith("Amusing 2027 participation invoice",reader.GetString(1));
            var note=JsonSerializer.Deserialize<ParticipationNote>(reader.GetString(2))!;
            Assert.Equal(DateOnly.FromDateTime(clock.Now).AddDays(42),note.PaymentDueOn);
            Assert.Equal(new DateOnly(2026,11,5),note.RegisteredOn);
            Assert.False(await reader.ReadAsync());
        }
        Assert.True((await service.ReadAsync(2027))!.Placements.Single().Queued);
        await using var count=source.CreateCommand("SELECT COUNT(*) FROM mijn_audit_log WHERE action='FestivalWaitlistPlaced'");
        Assert.Equal(1,Convert.ToInt32(await count.ExecuteScalarAsync()));
    }

    [DatabaseFact]
    public async Task PlacementAfterClosingNeedsExplicitDateAndKeepsItForCopies()
    {
        var clock=new FixedFestivalClock();
        var mail=await ReadyWaitlistedChoir(clock);
        clock.Now=new(2027,3,1,12,0,0);
        var service=PlacementService(clock);
        Assert.NotNull((await service.PlaceAsync(PlacementAdmin(),2027,new uint[]{1},null)).Error);
        Assert.True((await mail.ViewAsync(42,1,default))!.Registration!.Waitlisted);
        var due=new DateOnly(2027,3,15);
        Assert.Null((await service.PlaceAsync(PlacementAdmin(),2027,new uint[]{1},due)).Error);
        Assert.True(await mail.ProcessNextPlacementAsync(default));
        await using var read=source.CreateCommand("SELECT nota_gegevens FROM mijn_inschrijfmail WHERE soort='nota'");
        var note=JsonSerializer.Deserialize<ParticipationNote>((string)(await read.ExecuteScalarAsync())!)!;
        Assert.Equal(due,note.PaymentDueOn);
        Assert.Contains("15 maart 2027",note.PaymentTerm("nl"));
    }

    [DatabaseFact]
    public async Task FailedPlacementAuditRollsBackRegistrationAndPersistentMailRequest()
    {
        var clock=new FixedFestivalClock();
        var mail=await ReadyWaitlistedChoir(clock);
        await Execute("DROP TABLE mijn_audit_log");
        await Assert.ThrowsAsync<MySqlConnector.MySqlException>(()=>PlacementService(clock).PlaceAsync(PlacementAdmin(),2027,new uint[]{1},null));
        Assert.True((await mail.ViewAsync(42,1,default))!.Registration!.Waitlisted);
        await using var count=source.CreateCommand("SELECT COUNT(*) FROM mijn_wachtlijstplaatsingen");
        Assert.Equal(0,Convert.ToInt32(await count.ExecuteScalarAsync()));
    }

    [DatabaseFact]
    public async Task PlacementFailureCanRetryWithoutDuplicateMessages()
    {
        var clock=new FixedFestivalClock();
        var mail=await ReadyWaitlistedChoir(clock);
        Assert.Null((await PlacementService(clock).PlaceAsync(PlacementAdmin(),2027,new uint[]{1},null)).Error);
        await Execute("UPDATE ah_personen SET email='' WHERE persoon_id=42");
        await Assert.ThrowsAsync<InvalidOperationException>(()=>mail.ProcessNextPlacementAsync(default));
        await Execute("UPDATE ah_personen SET email='koor@example.invalid' WHERE persoon_id=42; UPDATE mijn_wachtlijstplaatsingen SET volgende_poging=UTC_TIMESTAMP(6)");
        Assert.True(await mail.ProcessNextPlacementAsync(default));
        Assert.False(await mail.ProcessNextPlacementAsync(default));
        await using var count=source.CreateCommand("SELECT COUNT(*) FROM mijn_inschrijfmail WHERE verzonden IS NULL");
        Assert.Equal(2,Convert.ToInt32(await count.ExecuteScalarAsync()));
    }

    [DatabaseFact]
    public async Task PlacementBatchWithInvalidContactRollsBackAndRequiresAuthenticatedAdmin()
    {
        var clock=new FixedFestivalClock();
        var mail=await ReadyWaitlistedChoir(clock);
        var service=PlacementService(clock);
        Assert.NotNull((await service.PlaceAsync(new ClaimsPrincipal(),2027,new uint[]{1},null)).Error);
        await Execute("INSERT INTO ah_inschrijvingen(festival_id,zanggroep_id,aantal_deelnemers,podiumsoort,ingeschreven,wachtlijst) VALUES(2027,2,20,'A','2026-11-05',1)");
        Assert.NotNull((await service.PlaceAsync(PlacementAdmin(),2027,new uint[]{1,2},null)).Error);
        Assert.True((await mail.ViewAsync(42,1,default))!.Registration!.Waitlisted);
        await using var count=source.CreateCommand("SELECT COUNT(*) FROM mijn_wachtlijstplaatsingen");
        Assert.Equal(0,Convert.ToInt32(await count.ExecuteScalarAsync()));
    }

    [DatabaseFact]
    public async Task CancellationBeforeProcessingStopsPlacementMessages()
    {
        var clock=new FixedFestivalClock();
        var mail=await ReadyWaitlistedChoir(clock);
        var service=PlacementService(clock);
        Assert.Null((await service.PlaceAsync(PlacementAdmin(),2027,new uint[]{1},null)).Error);
        await Execute("UPDATE ah_inschrijvingen SET afgehaakt='2026-11-06' WHERE festival_id=2027 AND zanggroep_id=1");
        Assert.True(await mail.ProcessNextPlacementAsync(default));
        Assert.False(await mail.ProcessNextPlacementAsync(default));
        Assert.NotNull((await service.ReadAsync(2027))!.Placements.Single().Problem);
        await using var count=source.CreateCommand("SELECT COUNT(*) FROM mijn_inschrijfmail WHERE verzonden IS NULL");
        Assert.Equal(0,Convert.ToInt32(await count.ExecuteScalarAsync()));
    }
}
