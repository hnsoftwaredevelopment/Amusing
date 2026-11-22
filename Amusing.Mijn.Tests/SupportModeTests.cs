using System.Net;
using System.Text.RegularExpressions;

namespace Amusing.Mijn.Tests;

public partial class LoginTests
{
    [DatabaseFact]
    public async Task BeheerAccountMustSelectChoirAndKeepsOwnAuditIdentity()
    {
        await Execute("INSERT INTO ah_beheer(user_id,username,role,PasswordHash) VALUES(42,'testbeheer','admin',@hash)", originalHash);
        var loginPage = await client.GetStringAsync("/account/beheer");
        var login = await PostAsync("/account/beheer-inloggen", loginPage, new()
        {
            ["username"] = "testbeheer", ["password"] = Password
        });
        Assert.Equal(HttpStatusCode.Redirect, login.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/koor/1")).StatusCode);

        var picker = await client.GetStringAsync("/");
        Assert.Contains("Verborgen Anderkoor", picker);
        var selected = await PostAsync("/ondersteuning/koor", picker, new() { ["group"] = "2" });
        Assert.Equal("/koor/2", selected.Headers.Location?.OriginalString);

        var profile = await client.GetStringAsync("/koor/2");
        Assert.Contains("Ondersteuningsmodus", profile);
        Assert.Contains("Algemene koorgegevens opslaan", profile);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/koor/1")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/account/uitnodigingen")).StatusCode);
        Assert.Contains("Gekoppelde personen", await client.GetStringAsync("/koor/2/personen"));

        var version = Regex.Match(profile, "name=\"Input.Version\" value=\"([^\"]+)\"").Groups[1].Value;
        Assert.NotEmpty(version);
        var save = await PostAsync("/koor/2", profile, new()
        {
            ["_handler"] = "choir-profile", ["Input.Version"] = WebUtility.HtmlDecode(version),
            ["Input.Name"] = "Testkoor hernoemd", ["Input.City"] = "Hengelo",
            ["Input.GenreId"] = "1", ["Input.Country"] = "NL"
        });
        Assert.Contains("De koorgegevens zijn opgeslagen", await save.Content.ReadAsStringAsync());
        await using var audit = source.CreateCommand("""
            SELECT COUNT(*) FROM mijn_audit_log
            WHERE actor_type='beheer' AND actor_id=42 AND group_id=2 AND action='ChoirProfileChanged'
            """);
        Assert.Equal(1, Convert.ToInt32(await audit.ExecuteScalarAsync()));

        await PostAsync("/ondersteuning/koor", profile, new());
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/koor/2")).StatusCode);
        await Execute("DELETE FROM ah_beheer WHERE user_id=42");
        Assert.Equal(HttpStatusCode.Redirect, (await client.GetAsync("/")).StatusCode);
    }

    private async Task<HttpResponseMessage> PostAsync(string url, string html, Dictionary<string,string> fields)
    {
        fields["__RequestVerificationToken"] = WebUtility.HtmlDecode(Regex.Match(html,
            "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value);
        Assert.NotEmpty(fields["__RequestVerificationToken"]);
        return await client.PostAsync(url, new FormUrlEncodedContent(fields));
    }
}
