using Amusing.Mijn.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace Amusing.Mijn.Tests;

public partial class LoginTests
{
    [DatabaseFact]
    public async Task UpdatedPriceQueuesOnlyNewNoteWithTemplatesAndOriginalPaymentDate()
    {
        await ReadyChoir();
        await Execute(await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory,"021-extra-mailsjablonen.sql")));
        await Execute("UPDATE mijn_mailingsjablonen SET bericht='De nota is verstuurd naar {RolNotaOntvanger} ({MailNotaOntvanger}).' WHERE soort='aanmelding' AND taal='nl'");
        var clock = new FixedFestivalClock();
        var service = new ChoirEnrollments(source,clock);
        var input = ValidEnrollment();
        Assert.Equal(EnrollmentResult.Saved,await service.RegisterAsync(42,1,input,default));
        await using (var confirmation = source.CreateCommand("SELECT bericht FROM mijn_inschrijfmail WHERE soort='koor'"))
            Assert.Equal("De nota is verstuurd naar Eerste contactpersoon (koor@example.invalid).",await confirmation.ExecuteScalarAsync());
        await Execute("UPDATE mijn_inschrijfmail SET verzonden=UTC_TIMESTAMP(6)");
        input.Version = (await service.ViewAsync(42,1,default))!.Registration!.Version;
        input.Headcount = 55;
        clock.Now = clock.Now.AddDays(10);
        Assert.Equal(EnrollmentResult.Saved,await service.UpdateAsync(42,1,input,default));
        await using(var cmd=source.CreateCommand("SELECT soort,bericht,nota_gegevens FROM mijn_inschrijfmail WHERE verzonden IS NULL"))
        await using(var reader=await cmd.ExecuteReaderAsync())
        {
            Assert.True(await reader.ReadAsync());
            Assert.Equal("nota",reader.GetString(0));
            Assert.DoesNotContain("{",reader.GetString(1));
            Assert.Contains("Hallo Testpersoon,",reader.GetString(1));
            var note=System.Text.Json.JsonSerializer.Deserialize<ParticipationNote>(reader.GetString(2))!;
            Assert.Equal(55u,note.Headcount);
            Assert.NotNull(note.RegisteredOn);
            Assert.True(note.RegisteredOn < note.IssuedOn);
            using var pdf = new Syncfusion.Pdf.Parsing.PdfLoadedDocument(ParticipationNotePdf.Render(note,true));
            Assert.Contains(note.PaymentTerm("nl"),pdf.Pages[0].ExtractText(true));
            Assert.False(await reader.ReadAsync());
        }
        await Execute("UPDATE mijn_inschrijfmail SET verzonden=UTC_TIMESTAMP(6)");
        input.Version = (await service.ViewAsync(42,1,default))!.Registration!.Version;
        input.Preference = input.Preference == "02" ? "20" : "02";
        Assert.Equal(EnrollmentResult.Saved,await service.UpdateAsync(42,1,input,default));
        await using var count=source.CreateCommand("SELECT COUNT(*) FROM mijn_inschrijfmail WHERE verzonden IS NULL");
        Assert.Equal(0,Convert.ToInt32(await count.ExecuteScalarAsync()));
    }

    [DatabaseFact]
    public async Task ConfirmationFollowsPrimaryContactsLanguageForBothRegistrationStatuses()
    {
        await ReadyChoir();
        await Execute("""
            UPDATE ah_personen SET actief=1 WHERE persoon_id=43;
            UPDATE ah_personen SET voornaam='Ada',tussenvoegsel='van',achternaam='Dijk' WHERE persoon_id=42;
            INSERT INTO ah_personen_rollen VALUES(43,1,'contactpersoon2');
            INSERT INTO mijn_taalvoorkeur VALUES(42,'de'),(43,'nl');
            UPDATE mijn_mailingsjablonen SET bericht=CONCAT('Hallo {Volledige naam},',CHAR(10),bericht)
            WHERE soort='aanmelding' AND taal='de';
            """);
        var service = new ChoirEnrollments(source, new FixedFestivalClock());
        Assert.Equal(EnrollmentResult.Saved, await service.RegisterAsync(43, 1, ValidEnrollment(), default));
        await using var subject = source.CreateCommand("SELECT onderwerp FROM mijn_inschrijfmail WHERE soort='koor'");
        await using var body = source.CreateCommand("SELECT bericht FROM mijn_inschrijfmail WHERE soort='koor'");
        Assert.StartsWith("Anmeldung Amusing 2027", Assert.IsType<string>(await subject.ExecuteScalarAsync()));
        Assert.StartsWith("Hallo Ada van Dijk,", Assert.IsType<string>(await body.ExecuteScalarAsync()));
        Assert.Contains("Die Rechnung für diese Anmeldung wurde separat an Sie geschickt (koor@example.invalid).", Assert.IsType<string>(await body.ExecuteScalarAsync()));
        await using var noteSubject = source.CreateCommand("SELECT onderwerp FROM mijn_inschrijfmail WHERE soort='nota'");
        Assert.StartsWith("Teilnahmerechnung Amusing 2027", Assert.IsType<string>(await noteSubject.ExecuteScalarAsync()));

        await Execute("DELETE FROM ah_inschrijvingen; DELETE FROM mijn_inschrijfmail; UPDATE ah_festivals SET wachtlijst=1 WHERE festival_id=2027; UPDATE mijn_taalvoorkeur SET taal='en' WHERE persoon_id=42");
        Assert.Equal(EnrollmentResult.Saved, await service.RegisterAsync(43, 1, ValidEnrollment(), default));
        Assert.StartsWith("Waiting list 2027", Assert.IsType<string>(await subject.ExecuteScalarAsync()));
        var waitlistBody = Assert.IsType<string>(await body.ExecuteScalarAsync());
        Assert.Contains("Your choir is on the waiting list.", waitlistBody);
        Assert.DoesNotContain("invoice", waitlistBody, StringComparison.OrdinalIgnoreCase);

        await Execute("""
            DELETE FROM ah_inschrijvingen;
            DELETE FROM mijn_inschrijfmail;
            UPDATE mijn_mailingsjablonen SET onderwerp='',bericht='' WHERE soort='wachtlijst' AND taal='en';
            UPDATE mijn_mailingsjablonen SET onderwerp='Aangepast: [koor]' WHERE soort='wachtlijst' AND taal='nl';
            """);
        Assert.Equal(EnrollmentResult.Saved, await service.RegisterAsync(43, 1, ValidEnrollment(), default));
        Assert.Equal("Aangepast: Eigen Testkoor", Assert.IsType<string>(await subject.ExecuteScalarAsync()));
        Assert.Contains("Het koor staat op de wachtlijst.", Assert.IsType<string>(await body.ExecuteScalarAsync()));
    }

    [DatabaseFact]
    public async Task TestWaitlistRegistrationQueuesConfirmationAndOrganisationMessageWithoutNote()
    {
        await ReadyChoir();
        await Execute("UPDATE ah_personen SET email='contact@example.invalid' WHERE persoon_id=42");
        await Execute("UPDATE ah_festivals SET wachtlijst=1 WHERE festival_id=2027");
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Mijn:IsTestEnvironment"] = "true"
        }).Build();
        var service = new ChoirEnrollments(source, new FixedFestivalClock(), configuration: config);

        Assert.Equal(EnrollmentResult.Saved, await service.RegisterAsync(42, 1, ValidEnrollment(), default));

        await using var command = source.CreateCommand("SELECT soort, onderwerp, ontvanger, bericht FROM mijn_inschrijfmail ORDER BY soort");
        await using var reader = await command.ExecuteReaderAsync();
        Assert.True(await reader.ReadAsync());
        Assert.Equal("koor", reader.GetString(0));
        Assert.StartsWith("Wachtlijst 2027", reader.GetString(1));
        Assert.Equal("contact@example.invalid", reader.GetString(2));
        Assert.Contains("We laten jullie weten of er een plek vrijkomt.", reader.GetString(3));
        Assert.True(await reader.ReadAsync());
        Assert.Equal("organisatie", reader.GetString(0));
        Assert.StartsWith("Nieuwe inschrijving:", reader.GetString(1));
        Assert.Equal("koren@amusing-hengelo.nl", reader.GetString(2));
        Assert.Contains("Geen nota bij een wachtlijstinschrijving.", reader.GetString(3));
        Assert.False(await reader.ReadAsync());
    }

    [DatabaseFact]
    public async Task NewRegistrationAfterRemovalRequeuesPreviouslySentMail()
    {
        await ReadyChoir();
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Mijn:IsTestEnvironment"] = "true"
        }).Build();
        var service = new ChoirEnrollments(source, new FixedFestivalClock(), configuration: config);
        Assert.Equal(EnrollmentResult.Saved, await service.RegisterAsync(42, 1, ValidEnrollment(), default));
        await Execute("UPDATE mijn_inschrijfmail SET verzonden=UTC_TIMESTAMP(6), pogingen=1; DELETE FROM ah_inschrijvingen WHERE zanggroep_id=1");

        Assert.Equal(EnrollmentResult.Saved, await service.RegisterAsync(42, 1, ValidEnrollment(), default));
        Assert.Equal(EnrollmentResult.AlreadyRegistered, await service.RegisterAsync(42, 1, ValidEnrollment(), default));

        await using var command = source.CreateCommand("SELECT COUNT(*), SUM(verzonden IS NULL AND pogingen=0), SUM(soort='nota' AND ontvanger_rol='contactpersoon1') FROM mijn_inschrijfmail");
        await using var reader = await command.ExecuteReaderAsync();
        Assert.True(await reader.ReadAsync());
        Assert.Equal(3, reader.GetInt32(0));
        Assert.Equal(3, reader.GetInt32(1));
        Assert.Equal(1, reader.GetInt32(2));
        await reader.DisposeAsync();
        await using var organisation = source.CreateCommand("SELECT bericht FROM mijn_inschrijfmail WHERE soort='organisatie'");
        var body = Assert.IsType<string>(await organisation.ExecuteScalarAsync());
        Assert.Contains("koor@example.invalid (contactpersoon 1, want penningmeester niet ingevuld)", body);
    }

    [DatabaseFact]
    public async Task RegistrationQueuesMailOnceAndRetriesAfterPickupFailure()
    {
        await ReadyChoir();
        await Execute("UPDATE ah_personen SET actief=1, email='penningmeester@example.invalid' WHERE persoon_id=43; INSERT INTO ah_personen_rollen VALUES(43,1,'penningmeester')");
        await Execute("INSERT INTO mijn_taalvoorkeur VALUES(42,'nl'),(43,'en')");
        await Execute("""
            UPDATE ah_personen SET voornaam='Ada & Co' WHERE persoon_id=42;
            UPDATE mijn_mailingsjablonen SET opmaak_html='<p>Hallo <strong>{Volledige naam}</strong></p><p>{Notamelding}</p>'
            WHERE soort='aanmelding' AND taal='nl';
            """);
        var service = new ChoirEnrollments(source, new FixedFestivalClock());
        Assert.Equal(EnrollmentResult.Saved, await service.RegisterAsync(42, 1, ValidEnrollment(), default));
        Assert.Equal(EnrollmentResult.AlreadyRegistered, await service.RegisterAsync(42, 1, ValidEnrollment(), default));
        await Execute("UPDATE mijn_taalvoorkeur SET taal='de' WHERE persoon_id=43");
        await Execute("UPDATE mijn_inschrijfmail SET volgende_poging=TIMESTAMPADD(HOUR, 1, UTC_TIMESTAMP(6)) WHERE soort='organisatie'");

        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var blocked = Path.Combine(directory, "blocked");
        await File.WriteAllTextAsync(blocked, "This file prevents creating the pickup directory.");
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["EnrollmentMail:Enabled"] = "true", ["Mijn:IsTestEnvironment"] = "true",
            ["EnrollmentMail:PickupDirectory"] = blocked
        }).Build();
        try
        {
            await CheckWorkerAsync("soort='nota' AND pogingen=1 AND verzonden IS NULL");
            await using (var dependent = source.CreateCommand("SELECT pogingen FROM mijn_inschrijfmail WHERE soort='koor' AND afhankelijk_van IS NOT NULL"))
                Assert.Equal(0, Convert.ToInt32(await dependent.ExecuteScalarAsync()));
            config["EnrollmentMail:PickupDirectory"] = directory;
            await Execute("UPDATE mijn_inschrijfmail SET volgende_poging=UTC_TIMESTAMP(6) WHERE soort='nota'");
            await CheckWorkerAsync("soort='nota' AND pogingen=2 AND verzonden IS NOT NULL");
            var note = MimeKit.MimeMessage.Load(Assert.Single(Directory.GetFiles(directory, "*.eml")));
            Assert.Equal("penningmeester@example.invalid", Assert.Single(note.To.Mailboxes).Address);
            Assert.StartsWith("[DEV Mijn] Amusing 2027 participation invoice", note.Subject);
            Assert.Contains("The total participation fee is €", note.TextBody);
            var attachment = Assert.IsType<MimeKit.MimePart>(Assert.Single(note.Attachments));
            Assert.Equal("application/pdf", attachment.ContentType.MimeType);
            using var pdfBytes = new MemoryStream();
            attachment.Content.DecodeTo(pdfBytes);
            using var pdf = new Syncfusion.Pdf.Parsing.PdfLoadedDocument(pdfBytes.ToArray());
            var pdfText = pdf.Pages[0].ExtractText(true);
            Assert.Contains("Test participation invoice", pdfText);
            Assert.Contains("Please do not make a payment.", pdfText);
            Assert.Equal(4, pdfText.Count(character => character == '€'));
            await CheckWorkerAsync("soort='koor' AND verzonden IS NOT NULL");
            await using var count = source.CreateCommand("SELECT COUNT(*) FROM mijn_inschrijfmail");
            Assert.Equal(4, Convert.ToInt32(await count.ExecuteScalarAsync()));

            var confirmation = Assert.Single(Directory.GetFiles(directory, "*.eml").Select(path => MimeKit.MimeMessage.Load(path)),
                message => !message.Attachments.Any());
            Assert.Equal("koor@example.invalid", Assert.Single(confirmation.To.Mailboxes).Address);
            Assert.Contains("separaat verstuurd naar de penningmeester", confirmation.TextBody);
            Assert.Contains("<strong>Ada &amp; Co</strong>", confirmation.HtmlBody);
            Assert.Contains("separaat verstuurd naar de penningmeester", confirmation.HtmlBody);
            await using var organisation = source.CreateCommand("""
                SELECT COUNT(*) FROM mijn_inschrijfmail o JOIN mijn_inschrijfmail k ON k.id=o.afhankelijk_van
                WHERE o.soort='organisatie' AND k.soort='koor'
                  AND o.bericht LIKE '%Bevestigingsmail is verstuurd naar: koor@example.invalid (contactpersoon 1)%'
                  AND o.bericht LIKE '%Nota is verstuurd naar: penningmeester@example.invalid (penningmeester)%'
                """);
            Assert.Equal(2, Convert.ToInt32(await organisation.ExecuteScalarAsync()));
            await using var recipients = source.CreateCommand("SELECT GROUP_CONCAT(ontvanger ORDER BY ontvanger SEPARATOR ',') FROM mijn_inschrijfmail WHERE soort='organisatie'");
            Assert.Equal("info@amusing-hengelo.nl,koren@amusing-hengelo.nl",await recipients.ExecuteScalarAsync());
            await using var audit = source.CreateCommand("SELECT COUNT(*) FROM mijn_audit_log WHERE action='ParticipationNoteSent' AND role_name='penningmeester'");
            Assert.Equal(1, Convert.ToInt32(await audit.ExecuteScalarAsync()));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }

        async Task CheckWorkerAsync(string filter)
        {
            using var worker = new EnrollmentMailWorker(source, config, NullLogger<EnrollmentMailWorker>.Instance);
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(7));
            await worker.StartAsync(timeout.Token);
            try
            {
                await using var command = source.CreateCommand("SELECT COUNT(*) FROM mijn_inschrijfmail WHERE " + filter);
                while (Convert.ToInt32(await command.ExecuteScalarAsync(timeout.Token)) != 1)
                    await Task.Delay(100, timeout.Token);
            }
            finally
            {
                await worker.StopAsync(default);
            }
        }
    }
}
