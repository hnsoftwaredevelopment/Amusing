using Amusing.Mijn.Services;
using Microsoft.Extensions.DependencyInjection;
namespace Amusing.Mijn.Tests;

public partial class LoginTests
{
    [DatabaseFact] public async Task EnrollmentConfirmationFormConfirmsReviewedData()
    {
        await ReadyChoir();
        var clock = (FixedFestivalClock)factory.Services.GetRequiredService<IFestivalClock>();
        var service = new ChoirEnrollments(source, clock);
        Assert.Equal(EnrollmentResult.Saved, await service.RegisterAsync(42, 1, ValidEnrollment(), default));
        clock.Now = new(2027, 3, 1);
        await Login();
        var html = await client.GetStringAsync("/koor/1/inschrijven");
        Assert.Contains("Inschrijving bevestigen", html);
        var token = System.Net.WebUtility.HtmlDecode(System.Text.RegularExpressions.Regex.Match(html,
            "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value);
        var registration = (await service.ViewAsync(42, 1, default))!.Registration!;
        var fields = new Dictionary<string, string>
        {
            ["_handler"] = "confirm-enrollment", ["__RequestVerificationToken"] = token,
            ["Confirmation.FestivalId"] = "2027", ["Confirmation.Version"] = registration.Version
        };
        var response = await client.PostAsync("/koor/1/inschrijven", new FormUrlEncodedContent(fields));
        response.EnsureSuccessStatusCode();
        Assert.Contains("De inschrijving is bevestigd.", await response.Content.ReadAsStringAsync());
        Assert.True((await service.ViewAsync(42, 1, default))!.Registration!.Confirmed);
    }

    [DatabaseFact] public async Task EnrollmentEditAfterClosingRejectsStaleEditsAndLocksOnConfirmation()
    {
        await ReadyChoir();
        var clock = new FixedFestivalClock();
        var service = new ChoirEnrollments(source, clock);
        var input = ValidEnrollment();
        Assert.Equal(EnrollmentResult.Saved, await service.RegisterAsync(42, 1, input, default));
        input.Version = (await service.ViewAsync(42, 1, default))!.Registration!.Version;
        var confirmation = new EnrollmentConfirmation { FestivalId = input.FestivalId, Version = input.Version };
        clock.Now = new(2027, 2, 28, 23, 59, 59);
        Assert.Equal(EnrollmentResult.TooEarly, await service.ConfirmAsync(42, 1, confirmation, default));

        clock.Now = new(2027, 3, 1);
        input.Headcount = 40;
        Assert.Equal(EnrollmentResult.Saved, await service.UpdateAsync(42, 1, input, default));
        Assert.Equal(EnrollmentResult.Conflict, await service.ConfirmAsync(42, 1, confirmation, default));
        Assert.Equal(EnrollmentResult.Conflict, await service.UpdateAsync(42, 1, input, default));

        var latest = (await service.ViewAsync(42, 1, default))!.Registration!;
        Assert.Equal(40u, latest.Headcount);
        confirmation.Version = latest.Version;
        Assert.Equal(EnrollmentResult.Confirmed, await service.ConfirmAsync(42, 1, confirmation, default));
        Assert.Equal(EnrollmentResult.AlreadyConfirmed, await service.ConfirmAsync(42, 1, confirmation, default));
        Assert.Equal(EnrollmentResult.Locked, await service.UpdateAsync(42, 1, input, default));
        await using var audit = source.CreateCommand("SELECT COUNT(*) FROM mijn_audit_log WHERE action='FestivalEnrollmentConfirmed'");
        Assert.Equal(1, Convert.ToInt32(await audit.ExecuteScalarAsync()));
    }

    [DatabaseFact] public async Task EnrollmentRespectsWaitlistPaymentCancellationAndAssignedStage()
    {
        await ReadyChoir();
        var clock = new FixedFestivalClock();
        var service = new ChoirEnrollments(source, clock);
        var input = ValidEnrollment();
        await Execute("UPDATE ah_festivals SET wachtlijst=1 WHERE festival_id=2027");
        Assert.Equal(EnrollmentResult.Saved, await service.RegisterAsync(42, 1, input, default));
        clock.Now = new(2027, 3, 1);
        var registration = (await service.ViewAsync(42, 1, default))!.Registration!;
        var confirmation = new EnrollmentConfirmation { FestivalId = input.FestivalId, Version = registration.Version };
        Assert.Equal(EnrollmentResult.Waitlisted, await service.ConfirmAsync(42, 1, confirmation, default));
        input.Version = registration.Version;
        input.Headcount = 36;
        Assert.Equal(EnrollmentResult.Saved, await service.UpdateAsync(42, 1, input, default));

        await Execute("UPDATE ah_inschrijvingen SET podiumkeuze_geforceerd=1,wachtlijst=0");
        input.Version = (await service.ViewAsync(42, 1, default))!.Registration!.Version;
        input.PodiumType = "B";
        Assert.Equal(EnrollmentResult.Invalid, await service.UpdateAsync(42, 1, input, default));

        await Execute("UPDATE ah_inschrijvingen SET betaald='2027-02-20'");
        registration = (await service.ViewAsync(42, 1, default))!.Registration!;
        Assert.False(registration.CanEdit);
        Assert.True(registration.CanConfirm);
        input.Version = registration.Version;
        Assert.Equal(EnrollmentResult.Locked, await service.UpdateAsync(42, 1, input, default));
        confirmation.Version = registration.Version;
        Assert.Equal(EnrollmentResult.Forbidden, await service.ConfirmAsync(42, 2, confirmation, default));
        Assert.Equal(EnrollmentResult.Confirmed, await service.ConfirmAsync(42, 1, confirmation, default));

        await Execute("UPDATE ah_inschrijvingen SET afgehaakt='2027-03-01',bevestigd=NULL");
        Assert.Equal(EnrollmentResult.Locked, await service.ConfirmAsync(42, 1, confirmation, default));
        Assert.Equal(EnrollmentResult.Locked, await service.UpdateAsync(42, 1, input, default));
    }

    [DatabaseFact] public async Task FailedConfirmationAuditRollsBackConfirmation()
    {
        await ReadyChoir();
        var clock = new FixedFestivalClock();
        var service = new ChoirEnrollments(source, clock);
        Assert.Equal(EnrollmentResult.Saved, await service.RegisterAsync(42, 1, ValidEnrollment(), default));
        clock.Now = new(2027, 3, 1);
        var registration = (await service.ViewAsync(42, 1, default))!.Registration!;
        await Execute("DROP TABLE mijn_audit_log");
        await Assert.ThrowsAsync<MySqlConnector.MySqlException>(() => service.ConfirmAsync(42, 1,
            new EnrollmentConfirmation { FestivalId = 2027, Version = registration.Version }, default));
        Assert.False((await service.ViewAsync(42, 1, default))!.Registration!.Confirmed);
    }
}
