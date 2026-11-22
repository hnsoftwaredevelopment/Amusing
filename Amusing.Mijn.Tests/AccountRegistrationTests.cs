using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Amusing.Mijn.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Amusing.Mijn.Tests;

public partial class LoginTests
{
    private async Task<(AccountRegistration Service,string Directory)> RegistrationService()
    {
        await Execute("""
            CREATE TABLE IF NOT EXISTS mijn_account_activation(token_hash BINARY(32) PRIMARY KEY,email VARCHAR(80) NOT NULL,
                first_name VARCHAR(40) NOT NULL,last_name VARCHAR(50) NOT NULL,expires_at DATETIME(6) NOT NULL);
            DELETE FROM mijn_account_activation;
            ALTER TABLE ah_personen MODIFY persoon_id INT UNSIGNED NOT NULL AUTO_INCREMENT;
            CREATE UNIQUE INDEX IF NOT EXISTS ix_registration_email ON ah_personen(email);
            """);
        var directory=Path.Combine(Path.GetTempPath(),"mijn-registration-"+Guid.NewGuid().ToString("N"));
        var config=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>
        {
            ["AccountRegistration:Enabled"]="true",["AccountRegistration:Origin"]="https://localhost",
            ["Mijn:IsTestEnvironment"]="true",["EnrollmentMail:PickupDirectory"]=directory
        }).Build();
        return (new(source,config,NullLogger<AccountRegistration>.Instance),directory);
    }

    private static string ActivationToken(string directory) => Regex.Match(
        MimeKit.MimeMessage.Load(Assert.Single(Directory.GetFiles(directory,"*.eml"))).TextBody!,
        "token=([0-9A-F]{64})").Groups[1].Value;

    [DatabaseFact]
    public async Task ContactWithoutCredentialsReceivesActivationAndKeepsIdentityAndChoirRoles()
    {
        var (service,directory)=await RegistrationService();
        try
        {
            await Execute("DELETE FROM ah_personen_wachtwoorden WHERE id=43; UPDATE ah_personen SET actief=1 WHERE persoon_id=43; INSERT INTO ah_personen_rollen VALUES(43,1,'penningmeester')");
            Assert.Equal(RegistrationRequestResult.Sent,await service.RequestAsync("Andere","Ingevoerde naam","inactive@example.invalid",default));
            Assert.True(Directory.Exists(directory));
            var token=ActivationToken(directory);Assert.NotEmpty(token);
            Assert.True(await service.ActivateAsync(token,Password,default));
            Assert.False(await service.ActivateAsync(token,Password,default));
            await using var read=source.CreateCommand("SELECT p.voornaam,p.actief,w.hash,(SELECT COUNT(*) FROM ah_personen_rollen WHERE persoon_id=43) FROM ah_personen p JOIN ah_personen_wachtwoorden w ON w.id=p.persoon_id WHERE p.persoon_id=43");
            await using var reader=await read.ExecuteReaderAsync();
            Assert.True(await reader.ReadAsync());Assert.Equal("Inactief",reader.GetString(0));Assert.True(reader.GetBoolean(1));
            Assert.True(BCrypt.Net.BCrypt.Verify(Password,reader.GetString(2)));Assert.Equal(1,reader.GetInt32(3));
        }
        finally {if(Directory.Exists(directory))Directory.Delete(directory,true);}
    }

    [DatabaseFact]
    public async Task RegistrationDoesNotSendForExistingAccountsOrInactiveContacts()
    {
        var (service,directory)=await RegistrationService();
        try
        {
            await Execute("DELETE FROM ah_personen_wachtwoorden WHERE id=43");
            Assert.Equal(RegistrationRequestResult.Unavailable,await service.RequestAsync("Test","Account","koor@example.invalid",default));
            Assert.Equal(RegistrationRequestResult.Unavailable,await service.RequestAsync("Test","Account","inactive@example.invalid",default));
            Assert.False(Directory.Exists(directory));
            Assert.Equal(originalHash,await new Amusing.Authentication.MySql.MySqlChoirPasswordStore(source).ReadHashAsync(42,default));
        }
        finally {if(Directory.Exists(directory))Directory.Delete(directory,true);}
    }

    [DatabaseFact]
    public async Task NewPersonCanActivateOnlyOnceEvenWithConcurrentRequests()
    {
        var (service,directory)=await RegistrationService();
        try
        {
            await service.RequestAsync("Nieuw","Account","new@example.invalid",default);
            var token=ActivationToken(directory);
            var results=await Task.WhenAll(service.ActivateAsync(token,Password,default),service.ActivateAsync(token,Password,default));
            Assert.Single(results,value=>value);
            await using var count=source.CreateCommand("SELECT COUNT(*) FROM ah_personen WHERE email='new@example.invalid'");
            Assert.Equal(1,Convert.ToInt32(await count.ExecuteScalarAsync()));
        }
        finally {if(Directory.Exists(directory))Directory.Delete(directory,true);}
    }

    [DatabaseFact]
    public async Task ActivationCannotResetAPasswordCreatedAfterTheRequestOrUseExpiredTokens()
    {
        var (service,directory)=await RegistrationService();
        try
        {
            await Execute("DELETE FROM ah_personen_wachtwoorden WHERE id=43; UPDATE ah_personen SET actief=1 WHERE persoon_id=43");
            await service.RequestAsync("Test","Account","inactive@example.invalid",default);
            Assert.True(Directory.Exists(directory));
            var token=ActivationToken(directory);
            await Execute("INSERT INTO ah_personen_wachtwoorden VALUES(43,@hash)",originalHash);
            Assert.False(await service.ActivateAsync(token,"Changed-password-2026!",default));
            await Execute("DELETE FROM ah_personen_wachtwoorden WHERE id=43; UPDATE mijn_account_activation SET expires_at=DATE_SUB(UTC_TIMESTAMP(6),INTERVAL 1 SECOND)");
            Assert.False(await service.ActivateAsync(token,Password,default));
        }
        finally {if(Directory.Exists(directory))Directory.Delete(directory,true);}
    }

    [DatabaseFact]
    public async Task RegistrationReportsMailFailureInsteadOfClaimingAnActivationWasSent()
    {
        var (service,directory)=await RegistrationService();
        await File.WriteAllTextAsync(directory,"This file blocks the pickup directory.");
        try
        {
            Assert.Equal(RegistrationRequestResult.MailFailed,await service.RequestAsync("Nieuwe","Persoon","new@example.invalid",default));
        }
        finally {File.Delete(directory);}
    }

    [DatabaseFact]
    public async Task RegistrationPageShowsAFailureWhenTheMailCannotBeSubmitted()
    {
        var (_,directory)=await RegistrationService();
        await File.WriteAllTextAsync(directory,"This file blocks the pickup directory.");
        using var host=factory.WithWebHostBuilder(builder=>builder.ConfigureAppConfiguration((_,config)=>config.AddInMemoryCollection(new Dictionary<string,string?>
        {
            ["AccountRegistration:Enabled"]="true",["AccountRegistration:Origin"]="https://localhost",
            ["Mijn:IsTestEnvironment"]="true",["EnrollmentMail:PickupDirectory"]=directory
        })));
        using var browser=host.CreateClient(new WebApplicationFactoryClientOptions {AllowAutoRedirect=false,BaseAddress=new Uri("https://localhost")});
        try
        {
            var page=await browser.GetStringAsync("/account/registreren");
            var token=System.Net.WebUtility.HtmlDecode(Regex.Match(page,"name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value);
            var response=await browser.PostAsync("/account/registratie-aanvragen",new FormUrlEncodedContent(new Dictionary<string,string>
            { ["first"]="Nieuw",["last"]="Account",["email"]="new@example.invalid",["__RequestVerificationToken"]=token }));
            Assert.Equal("/account/registreren?status=mailfout",response.Headers.Location?.OriginalString);
            Assert.Contains("De activatielink kon niet worden verstuurd",await browser.GetStringAsync(response.Headers.Location!));
        }
        finally {File.Delete(directory);}
    }
}
