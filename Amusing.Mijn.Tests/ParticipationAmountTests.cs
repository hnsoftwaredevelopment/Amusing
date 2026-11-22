using Amusing.Mijn.Services;

namespace Amusing.Mijn.Tests;

public partial class LoginTests
{
    [DatabaseFact]
    public async Task SavedRegistrationUsesEditionStaffelStagePriceAndVat()
    {
        await ReadyChoir();
        await Execute("""
            UPDATE ah_podia_typen SET prijs=25 WHERE type='A' AND versie=2026;
            UPDATE ah_festivals SET btw_percentage=21 WHERE festival_id=2027;
            """);

        var service = new ChoirEnrollments(source, new FixedFestivalClock());
        var input = ValidEnrollment();
        input.Headcount = 20;
        Assert.Equal(EnrollmentResult.Saved, await service.RegisterAsync(42, 1, input, default));
        var first = (await service.ViewAsync(42, 1, default))!.ParticipationAmount;
        Assert.NotNull(first);
        Assert.Equal(75m, first.TotaalInclBtw);

        await Execute("UPDATE ah_inschrijvingen SET aantal_deelnemers=21 WHERE festival_id=2027 AND zanggroep_id=1");
        var second = (await service.ViewAsync(42, 1, default))!.ParticipationAmount;
        Assert.NotNull(second);
        Assert.Equal(100m, second.TotaalInclBtw);
        Assert.Equal(21m, second.BtwPercentage);
        Assert.Equal(17.36m, second.Btw);

        await Execute("UPDATE ah_festivals SET btw_percentage=NULL WHERE festival_id=2027");
        Assert.Null((await service.ViewAsync(42, 1, default))!.ParticipationAmount);
    }
}
