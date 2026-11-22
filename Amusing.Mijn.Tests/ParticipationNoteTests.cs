using Amusing.Mijn.Services;

namespace Amusing.Mijn.Tests;

public partial class LoginTests
{
    [DatabaseFact]
    public async Task HistoricalInvoiceCopyUsesLatestRegistrationAndEditionRatesForTreasurer()
    {
        await ReadyChoir();
        await Execute(await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "history-dev-schema.sql")));
        await Execute("""
            INSERT INTO ah_festivals(festival_id,festivaldatum,start_inschrijving,eind_inschrijving,btw_percentage)
            VALUES(2025,'2025-06-07','2024-11-01','2025-03-01',9);
            INSERT INTO ah_festival_deelnemerstarieven VALUES(2025,1,60),(2025,21,85);
            INSERT INTO ah_podia_typen(type,versie,prijs,beschrijving) VALUES('B',2025,20,'Oud podium B');
            INSERT INTO ah_inschrijvingen(festival_id,zanggroep_id,aantal_deelnemers,podiumsoort,ingeschreven)
            VALUES(2025,1,15,'A','2024-12-01');
            UPDATE ah_inschrijvingen SET aantal_deelnemers=25,podiumsoort='B' WHERE festival_id=2025 AND zanggroep_id=1;
            """);
        var service = new ChoirEnrollments(source, new FixedFestivalClock());
        Assert.Equal("geen-toegang", await service.QueueNoteCopyAsync(42, 2, 2025, default));
        Assert.Equal("geen-penningmeester", await service.QueueNoteCopyAsync(42, 1, 2025, default));
        await Execute("UPDATE ah_personen SET actief=1 WHERE persoon_id=43; INSERT INTO ah_personen_rollen VALUES(43,1,'penningmeester'); INSERT INTO mijn_taalvoorkeur VALUES(43,'en')");
        Assert.Equal("klaargezet", await service.QueueNoteCopyAsync(43, 1, 2025, default));
        Assert.Equal("al-klaargezet", await service.QueueNoteCopyAsync(42, 1, 2025, default));

        await using var command = source.CreateCommand("SELECT ontvanger, onderwerp, nota_gegevens FROM mijn_inschrijfmail WHERE soort='kopienota'");
        await using var reader = await command.ExecuteReaderAsync();
        Assert.True(await reader.ReadAsync());
        Assert.Equal("inactive@example.invalid", reader.GetString(0));
        Assert.StartsWith("Copy of the Amusing 2025 participation invoice", reader.GetString(1));
        var note = System.Text.Json.JsonSerializer.Deserialize<ParticipationNote>(reader.GetString(2))!;
        Assert.Equal(25u, note.Headcount);
        Assert.Equal("B", note.Stage);
        Assert.Equal(105m, note.Amount.TotaalInclBtw);
        Assert.True(note.IsCopy);
        Assert.False(await reader.ReadAsync());
        await reader.DisposeAsync();

        await Login();
        var history = await client.GetStringAsync("/koor/1/deelnamehistorie");
        Assert.Contains("/koor/1/deelnamehistorie/2025/kopie-nota", history);
        Assert.Contains("Kopie-nota versturen", history);
    }

    [DatabaseFact]
    public async Task ConfirmedNoteKeepsAmountsAndOnlyLinkedChoirCanDownload()
    {
        await ReadyChoir();
        var clock = new FixedFestivalClock();
        var service = new ChoirEnrollments(source, clock);
        var input = ValidEnrollment();
        input.Headcount = 20;
        input.PodiumType = "B";
        await Execute("UPDATE ah_podia_typen SET prijs=25 WHERE type='B' AND versie=2026");
        Assert.Equal(EnrollmentResult.Saved, await service.RegisterAsync(42, 1, input, default));
        Assert.Null(await service.NoteAsync(42, 1, 2027, default));

        clock.Now = new(2027, 3, 1);
        var registration = (await service.ViewAsync(42, 1, default))!.Registration!;
        Assert.Equal(EnrollmentResult.Confirmed, await service.ConfirmAsync(42, 1,
            new() { FestivalId = 2027, Version = registration.Version }, default));
        await Execute("UPDATE ah_festival_deelnemerstarieven SET bedrag_incl_btw=99 WHERE festival_id=2027");
        var note = await service.NoteAsync(42, 1, 2027, default);
        Assert.Equal(75m, note!.Amount.TotaalInclBtw);

        await Login();
        using var response = await client.GetAsync("/koor/1/deelname-nota/2027");
        Assert.Equal("application/pdf", response.Content.Headers.ContentType?.MediaType);
        Assert.True(response.Headers.CacheControl!.NoStore);
        var pdf = await response.Content.ReadAsByteArrayAsync();
        Assert.StartsWith("%PDF-", System.Text.Encoding.ASCII.GetString(pdf, 0, 5));
        using var document = new Syncfusion.Pdf.Parsing.PdfLoadedDocument(pdf);
        var text = document.Pages[0].ExtractText(true);
        Assert.Equal(4, text.Count(character => character == '€'));
        Assert.DoesNotContain("EUR", text);
        await Execute("INSERT INTO mijn_taalvoorkeur VALUES(42,'de')");
        var german = await client.GetByteArrayAsync("/koor/1/deelname-nota/2027");
        using var germanDocument = new Syncfusion.Pdf.Parsing.PdfLoadedDocument(german);
        var germanText = germanDocument.Pages[0].ExtractText(true);
        Assert.Contains("Test-Teilnahmerechnung", germanText);
        Assert.Contains("Bühnentyp B", germanText);
        Assert.Contains("75,00", germanText);
        if (Environment.GetEnvironmentVariable("MIJN_PDF_PREVIEW") is { } preview)
            await File.WriteAllBytesAsync(preview, pdf);
        Assert.Equal(System.Net.HttpStatusCode.NotFound,
            (await client.GetAsync("/koor/2/deelname-nota/2027")).StatusCode);
        await Execute("DELETE FROM ah_personen_rollen WHERE persoon_id=42 AND zanggroep_id=1");
        Assert.Equal(System.Net.HttpStatusCode.NotFound,
            (await client.GetAsync("/koor/1/deelname-nota/2027")).StatusCode);
    }
}
