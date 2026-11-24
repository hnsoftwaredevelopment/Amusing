using System.Net;

namespace Amusing.Mijn.Tests;

public partial class LoginTests
{
    [DatabaseFact]
    public async Task AdministratorCanSearchByMiddleOfChoirNameAndSelectResult()
    {
        await Execute("INSERT INTO ah_beheer(user_id,username,role,PasswordHash) VALUES(42,'zoekbeheer','admin',@hash)", originalHash);
        await PostAsync("/account/beheer-inloggen", await client.GetStringAsync("/account/beheer"), new()
        { ["username"]="zoekbeheer", ["password"]=Password });
        var results = await client.GetStringAsync("/?zoek=ANDER");
        Assert.Contains("Verborgen Anderkoor", results);
        Assert.Matches("(?s)<tr[^>]*hidden[^>]*>.*?Eigen Testkoor", results);
        Assert.Contains("type=\"search\"", results);
        Assert.Contains("id=\"support-grid\"", results);
        Assert.Matches("<th[^>]*scope=\"col\"[^>]*>Standplaats</th>", results);
        Assert.DoesNotContain("profile-card person-card", results);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsync("/ondersteuning/koor",
            new FormUrlEncodedContent(new Dictionary<string,string> { ["group"]="2" }))).StatusCode);
        client.DefaultRequestHeaders.Add("Cookie", "Amusing.Mijn.Language=de");
        Assert.Contains("Chor unterstützen", WebUtility.HtmlDecode(await client.GetStringAsync("/?zoek=Ander")));
        client.DefaultRequestHeaders.Remove("Cookie");
        client.DefaultRequestHeaders.Add("Cookie", "Amusing.Mijn.Language=en");
        Assert.Contains("Support a choir", await client.GetStringAsync("/?zoek=Ander"));
        client.DefaultRequestHeaders.Remove("Cookie");
        var selected = await PostAsync("/ondersteuning/koor", results, new() { ["group"]="2" });
        Assert.Equal("/koor/2", selected.Headers.Location?.OriginalString);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/koor/1")).StatusCode);
        Assert.Contains("Geen koren gevonden", await client.GetStringAsync("/?zoek=nietsgevonden"));
        Assert.Contains("Eigen Testkoor", await client.GetStringAsync("/"));
    }

    [DatabaseFact]
    public async Task ChoirSearchTreatsSqlWildcardsLiterallyAndIsUnavailableToChoirMembers()
    {
        var accounts = new Amusing.Mijn.Services.SupportAccounts(source);
        Assert.Empty(await accounts.GroupsAsync(default, "%"));
        Assert.Empty(await accounts.GroupsAsync(default, "_"));
        await Login();
        var home = await client.GetStringAsync("/?zoek=Ander");
        Assert.Contains("Eigen Testkoor", home);
        Assert.DoesNotContain("Verborgen Anderkoor", home);
        Assert.DoesNotContain("id=\"choir-search\"", home);
        Assert.Equal(HttpStatusCode.Forbidden, (await PostAsync("/ondersteuning/koor", home, new() { ["group"]="2" })).StatusCode);
    }
}
