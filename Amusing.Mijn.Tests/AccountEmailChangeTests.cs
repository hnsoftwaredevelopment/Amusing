using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Amusing.Mijn.Tests;

public partial class LoginTests
{
    [DatabaseFact]
    public async Task NewEmailRequiresConfirmationAndInvalidatesOldRecoveryLinks()
    {
        foreach (var migration in new[] { "013-password-reset.sql", "014-beheer-wachtwoordherstel.sql", "020-emailwijziging.sql" })
            await Execute(await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, migration)));
        await Execute("DELETE FROM mijn_emailwijziging; DELETE FROM mijn_password_reset; CREATE UNIQUE INDEX IF NOT EXISTS ix_test_email ON ah_personen(email)");
        var pickup = Path.Combine(Path.GetTempPath(), "mijn-mail-" + Guid.NewGuid().ToString("N"));
        using var host = factory.WithWebHostBuilder(builder => builder.ConfigureAppConfiguration((_, config) =>
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AccountRegistration:Origin"] = "https://localhost",
                ["EnrollmentMail:TestSmtpEnabled"] = "true",
                ["EnrollmentMail:PickupDirectory"] = pickup
            })));
        client.Dispose();
        client = host.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false, BaseAddress = new Uri("https://localhost") });

        try
        {
            Assert.Equal("/", (await Login()).Headers.Location?.OriginalString);
            var contacts = await client.GetStringAsync("/account/contactgegevens");
            Assert.Contains("koor@example.invalid", contacts);
            var csrf = ReadToken(contacts);
            const string requestPath = "/account/e-mail-aanvragen";
            const string confirmPath = "/account/e-mail-wijziging-bevestigen";
            Assert.Equal("/account/contactgegevens?email=in-gebruik",
                (await PostAsync(requestPath, csrf, "email", "inactive@example.invalid")).Headers.Location?.OriginalString);
            Assert.False(Directory.Exists(pickup));
            Assert.Equal("/account/contactgegevens?email=klaargezet",
                (await PostAsync(requestPath, csrf, "email", "nieuw@example.com")).Headers.Location?.OriginalString);
            var mail = MimeKit.MimeMessage.Load(Assert.Single(Directory.GetFiles(pickup, "*.eml")));
            Assert.Equal("nieuw@example.com", Assert.Single(mail.To.Mailboxes).Address);
            var token = Regex.Match(mail.TextBody!, @"token=([0-9A-F]{64})").Groups[1].Value;
            Assert.NotEmpty(token);
            Assert.Contains("koor@example.invalid", await client.GetStringAsync("/account/contactgegevens"));
            Assert.Contains("failed=1", (await Login("nieuw@example.com")).Headers.Location?.OriginalString);

            await Execute("INSERT INTO mijn_password_reset VALUES(UNHEX(REPEAT('55',32)),42,REPEAT('A',64),DATE_ADD(UTC_TIMESTAMP(6),INTERVAL 1 HOUR),'choir_person')");
            var confirm = await client.GetStringAsync("/account/e-mail-bevestigen?token=" + token);
            Assert.Contains("Nieuw e-mailadres bevestigen", confirm);
            Assert.Equal("/account/contactgegevens?email=gewijzigd",
                (await PostAsync(confirmPath, ReadToken(confirm), "token", token)).Headers.Location?.OriginalString);
            Assert.Equal("/account/contactgegevens?email=verlopen",
                (await PostAsync(confirmPath, csrf, "token", token)).Headers.Location?.OriginalString);
            Assert.Contains("nieuw@example.com", await client.GetStringAsync("/account/contactgegevens"));
            Assert.Contains("failed=1", (await Login()).Headers.Location?.OriginalString);
            Assert.Equal("/", (await Login("nieuw@example.com")).Headers.Location?.OriginalString);
            await using var reset = source.CreateCommand("SELECT COUNT(*) FROM mijn_password_reset WHERE person_id=42");
            Assert.Equal(0, Convert.ToInt32(await reset.ExecuteScalarAsync()));
        }
        finally { if (Directory.Exists(pickup)) Directory.Delete(pickup, recursive: true); }

        Task<HttpResponseMessage> PostAsync(string path, string csrf, string field, string value) =>
            client.PostAsync(path, new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = csrf,
                [field] = value
            }));

        static string ReadToken(string html) => WebUtility.HtmlDecode(Regex.Match(html,
            "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value);
    }
}
