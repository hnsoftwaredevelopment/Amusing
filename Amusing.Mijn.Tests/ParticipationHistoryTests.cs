namespace Amusing.Mijn.Tests;

public partial class LoginTests
{
    [DatabaseFact]
    public async Task HistoryPageShowsOnlyPublishedPerformancesForOwnChoir()
    {
        foreach (var file in new[] { "history-dev-schema.sql", "history-dev-fixtures.sql" })
            await Execute(await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, file)));

        await Login();
        var page = await client.GetStringAsync("/koor/1/deelnamehistorie");
        Assert.Contains("Amusing 2026", page);
        Assert.Contains("Afgemeld", page);
        Assert.Contains("Amusing 2025", page);
        Assert.Contains("Testpodium Markt", page);
        Assert.Contains("Testpodium Bibliotheek", page);
        Assert.DoesNotContain("Testpodium Concept", page);

        await Execute("UPDATE ah_festivals SET planning_publiceren=1 WHERE festival_id=2025");
        Assert.Contains("Testpodium Concept", await client.GetStringAsync("/koor/1/deelnamehistorie"));
        Assert.DoesNotContain("Testpodium Markt", await client.GetStringAsync("/koor/2/deelnamehistorie"));

        await Execute("INSERT INTO ah_personen_rollen VALUES(42,2,'zanger')");
        var waitlisted = await client.GetStringAsync("/koor/2/deelnamehistorie");
        Assert.Contains("Op de wachtlijst", waitlisted);
        Assert.DoesNotContain("Testpodium Concept", waitlisted);
    }
}
