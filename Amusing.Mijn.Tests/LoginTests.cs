using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text.RegularExpressions;
using Amusing.Authentication.MySql;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using MySqlConnector;

namespace Amusing.Mijn.Tests;

public sealed class DatabaseFactAttribute : FactAttribute
{
    public DatabaseFactAttribute() { if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("MIJN_TEST_CONNECTION"))) Skip = "Set MIJN_TEST_CONNECTION to an isolated mijn_test_ database."; }
}
public partial class LoginTests : IAsyncLifetime
{
    private const string Password = "TestKoor-2026!";
    private MySqlDataSource source = null!;
    private WebApplicationFactory<Program> factory = null!;
    private HttpClient client = null!;
    private string originalHash = "";
    public async Task InitializeAsync()
    {
        var connection = Environment.GetEnvironmentVariable("MIJN_TEST_CONNECTION");
        if (string.IsNullOrEmpty(connection)) return;
        var settings = new MySqlConnectionStringBuilder(connection);
        if (!settings.Database.StartsWith("mijn_test_", StringComparison.Ordinal) || settings.Server != "127.0.0.1") throw new InvalidOperationException("Tests require an isolated mijn_test_ schema via loopback.");
        source = new MySqlDataSource(connection);
        await Execute("CREATE TABLE IF NOT EXISTS ah_personen (persoon_id INT UNSIGNED PRIMARY KEY,voornaam VARCHAR(40),tussenvoegsel VARCHAR(20),achternaam VARCHAR(50),email VARCHAR(80),actief TINYINT); CREATE TABLE IF NOT EXISTS ah_personen_wachtwoorden(id INT UNSIGNED PRIMARY KEY,hash VARCHAR(255) COLLATE utf8mb3_general_ci); CREATE TABLE IF NOT EXISTS ah_zanggroepen(zanggroep_id INT UNSIGNED PRIMARY KEY,naam VARCHAR(80),genre_id TINYINT UNSIGNED,standplaats VARCHAR(50),land VARCHAR(3),website VARCHAR(255),beschrijving TEXT,rekeningnr VARCHAR(50),actief TINYINT NOT NULL DEFAULT 1,foto MEDIUMBLOB NULL,logo MEDIUMBLOB NULL); CREATE TABLE IF NOT EXISTS ah_personen_rollen(persoon_id INT UNSIGNED,zanggroep_id INT UNSIGNED,rol VARCHAR(30),PRIMARY KEY(persoon_id,zanggroep_id,rol),INDEX ix_group(zanggroep_id,persoon_id,rol)); DELETE FROM ah_personen_rollen; DELETE FROM ah_zanggroepen; DELETE FROM ah_personen_wachtwoorden; DELETE FROM ah_personen;");
        await Execute("CREATE TABLE IF NOT EXISTS mijn_audit_log(id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,occurred_at DATETIME(6) NOT NULL,actor_type VARCHAR(20) NOT NULL,actor_id INT UNSIGNED NULL,action VARCHAR(40) NOT NULL,group_id INT UNSIGNED NULL,changed_fields VARCHAR(255) NULL,target_person_id INT UNSIGNED NULL,role_name VARCHAR(30) NULL); DELETE FROM mijn_audit_log;");
        await Execute("CREATE TABLE IF NOT EXISTS ah_contactgegevens(persoon_id INT UNSIGNED PRIMARY KEY,telefoon_vast VARCHAR(15),telefoon_mobiel VARCHAR(15)); CREATE TABLE IF NOT EXISTS mijn_role_invitations(id VARCHAR(32) PRIMARY KEY,group_id INT UNSIGNED,person_id INT UNSIGNED,role_name VARCHAR(30),created_by INT UNSIGNED,created_at DATETIME(6),expires_at DATETIME(6),accepted_at DATETIME(6) NULL,cancelled_at DATETIME(6) NULL); DELETE FROM mijn_role_invitations; DELETE FROM ah_contactgegevens;");
        await InitializeEnrollments();
        originalHash = BCrypt.Net.BCrypt.HashPassword(Password, 10).Replace("$2a$", "$2y$");
        await Execute("INSERT INTO ah_personen(persoon_id,voornaam,email,actief) VALUES(42,'Testpersoon','koor@example.invalid',1),(43,'Inactief','inactive@example.invalid',0); INSERT INTO ah_personen_wachtwoorden VALUES(42,@hash),(43,@hash); INSERT INTO ah_zanggroepen(zanggroep_id,naam) VALUES(1,'Eigen Testkoor'),(2,'Verborgen Anderkoor'); INSERT INTO ah_personen_rollen VALUES(42,1,'contactpersoon1');", originalHash);
        await Execute("CREATE TABLE IF NOT EXISTS ah_genres(genre_id INT PRIMARY KEY,nl VARCHAR(30)); CREATE TABLE IF NOT EXISTS ah_landen(code VARCHAR(3) PRIMARY KEY,naam VARCHAR(255)); CREATE TABLE IF NOT EXISTS ah_zanggroep_details(id INT UNSIGNED PRIMARY KEY,email VARCHAR(80)); INSERT IGNORE INTO ah_genres VALUES(1,'Gemengd'); INSERT IGNORE INTO ah_landen VALUES('NL','Nederland'); DELETE FROM ah_zanggroep_details; UPDATE ah_zanggroepen SET genre_id=1,standplaats='Hengelo',land='NL',website='',beschrijving='',rekeningnr='';");
        factory = new WebApplicationFactory<Program>().WithWebHostBuilder(b => b.UseEnvironment("Development").ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string,string?> { ["ConnectionStrings:Mijn"] = connection, ["Mijn:IsTestEnvironment"] = "true" })).ConfigureTestServices(services=>services.AddSingleton<Amusing.Mijn.Services.IFestivalClock>(new FixedFestivalClock())));
        client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false, BaseAddress = new Uri("https://localhost") });
    }
    public async Task DisposeAsync()
    {
        client?.Dispose();
        if (factory is not null) await factory.DisposeAsync();
        if (source is not null) await source.DisposeAsync();
    }
    private async Task Execute(string sql, string? hash = null)
    {
        await using var cmd = source.CreateCommand(sql);
        if (hash is not null) Add(cmd, "@hash", hash);
        await cmd.ExecuteNonQueryAsync();
    }
    private async Task<HttpResponseMessage> Login(string email="koor@example.invalid", string password=Password)
    {
        var page = await client.GetStringAsync("/account/login");
        var token = WebUtility.HtmlDecode(Regex.Match(page, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value);
        Assert.NotEmpty(token);
        return await client.PostAsync("/account/sign-in", new FormUrlEncodedContent(new Dictionary<string,string> { ["email"]=email,["password"]=password,["__RequestVerificationToken"]=token }));
    }
    [DatabaseFact] public async Task LoginUpgradesPhpHashAndOnlyShowsLinkedChoir()
    {
        var response = await Login();
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/",response.Headers.Location?.OriginalString);
        var store = new MySqlChoirPasswordStore(source);
        var updated = await store.ReadHashAsync(42, default);
        Assert.NotEqual(originalHash, updated);
        Assert.True(BCrypt.Net.BCrypt.Verify(Password,updated));
        var home = await client.GetStringAsync("/");
        Assert.Contains("Eigen Testkoor",home);
        Assert.DoesNotContain("Verborgen Anderkoor",home);
        var forbidden = await client.GetStringAsync("/koor/2");
        Assert.Contains("Koor niet beschikbaar",forbidden);
        Assert.DoesNotContain("Verborgen Anderkoor",forbidden);
        Assert.Contains("Eigen Testkoor",await client.GetStringAsync("/koor/1"));
    }
    [DatabaseFact] public async Task WrongPasswordAndInactiveAccountDoNotLogin()
    {
        Assert.Contains("failed=1",(await Login(password:"wrong")).Headers.Location!.OriginalString);
        Assert.Contains("failed=1",(await Login(email:"inactive@example.invalid")).Headers.Location!.OriginalString);
        Assert.Equal(originalHash, await new MySqlChoirPasswordStore(source).ReadHashAsync(42,default));
        Assert.Equal(HttpStatusCode.Redirect,(await client.GetAsync("/")).StatusCode);
    }
    [DatabaseFact] public async Task AntiforgeryRejectsLoginAndLogoutWithoutToken()
    {
        Assert.Equal(HttpStatusCode.BadRequest,(await client.PostAsync("/account/sign-in",new FormUrlEncodedContent(new Dictionary<string,string> { ["email"]="koor@example.invalid",["password"]=Password }))).StatusCode);
        await Login();
        Assert.Equal(HttpStatusCode.BadRequest,(await client.PostAsync("/account/logout", new FormUrlEncodedContent(new Dictionary<string,string>()))).StatusCode);
    }
    [DatabaseFact] public async Task ResetInvalidatesExistingSession()
    {
        await Login();
        await Execute("UPDATE ah_personen_wachtwoorden SET hash=@hash WHERE id=42",BCrypt.Net.BCrypt.HashPassword("New-2026!",11));
        Assert.Equal(HttpStatusCode.Redirect,(await client.GetAsync("/")).StatusCode);
    }
    [DatabaseFact] public async Task ProviderCompareAndSwapRejectsCaseMismatchAndStaleHash()
    {
        var store = new MySqlChoirPasswordStore(source);
        Assert.False(await store.TryReplaceHashAsync(42,originalHash.ToUpperInvariant(),"replacement",default));
        Assert.True(await store.TryReplaceHashAsync(42,originalHash,"replacement",default));
        Assert.False(await store.TryReplaceHashAsync(42,originalHash,"stale",default));
        Assert.Equal("replacement",await store.ReadHashAsync(42,default));
        Assert.Equal(originalHash,await store.ReadHashAsync(43,default));
    }
    [DatabaseFact] public async Task LogoutEndsSession()
    {
        await Login();
        var home = await client.GetStringAsync("/");
        var token = WebUtility.HtmlDecode(Regex.Match(home, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value);
        var response = await client.PostAsync("/account/logout",new FormUrlEncodedContent(new Dictionary<string,string>{["__RequestVerificationToken"]=token}));
        Assert.Equal(HttpStatusCode.Redirect,response.StatusCode);
        Assert.Equal(HttpStatusCode.Redirect,(await client.GetAsync("/")).StatusCode);
    }

    [DatabaseFact] public async Task AuditContainsUpgradeAndLoginWithActor()
    {
        await Login();
        await using var command = source.CreateCommand("SELECT action,actor_type,actor_id FROM mijn_audit_log ORDER BY id");
        await using var reader = await command.ExecuteReaderAsync();
        Assert.True(await reader.ReadAsync());
        Assert.Equal("PasswordUpgraded",reader.GetString(0));
        Assert.Equal("choir_person",reader.GetString(1));
        Assert.Equal(42u,reader.GetFieldValue<uint>(2));
        Assert.True(await reader.ReadAsync());
        Assert.Equal("LoginSucceeded",reader.GetString(0));
        Assert.False(await reader.ReadAsync());
    }
    [DatabaseFact] public async Task AuditFailureRollsBackPasswordUpgrade()
    {
        await Execute("DROP TABLE mijn_audit_log");
        var store = new MySqlChoirPasswordStore(source);
        await Assert.ThrowsAsync<MySqlException>(() => store.TryReplaceHashAsync(42,originalHash,"replacement",default));
        Assert.Equal(originalHash,await store.ReadHashAsync(42,default));
    }
    private static void Add(System.Data.Common.DbCommand cmd, string name, object value) { var p=cmd.CreateParameter(); p.ParameterName=name; p.Value=value; cmd.Parameters.Add(p); }
}
