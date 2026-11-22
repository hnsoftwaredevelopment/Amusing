using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
using Syncfusion.Drawing;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;

namespace Amusing.Mijn.Services;

public static class ParticipationNotePdf
{
    public static void MapParticipationNotes(this WebApplication app)
    {
        app.MapGet("/koor/{group:long}/deelname-nota/{festival:long}", async (
            long group, long festival, HttpContext context, ChoirEnrollments enrollments, IConfiguration configuration,
            SiteLanguage siteLanguage) =>
        {
            if (group is <= 0 or > uint.MaxValue || festival is <= 0 or > uint.MaxValue ||
                !uint.TryParse(context.User.FindFirstValue(ClaimTypes.NameIdentifier), out var actor))
                return Results.NotFound();

            var note = await enrollments.NoteAsync(actor, (uint)group, (uint)festival, context.RequestAborted, await siteLanguage.GetAsync());
            if (note is null)
                return Results.NotFound();

            context.Response.Headers.CacheControl = "private, no-store";
            return Results.File(Render(note, configuration.GetValue<bool>("Mijn:IsTestEnvironment"), await siteLanguage.GetAsync()),
                "application/pdf", $"deelname-nota-{note.Reference}.pdf");
        }).RequireAuthorization();

        app.MapPost("/koor/{group:long}/deelnamehistorie/{festival:long}/kopie-nota", async (
            long group, long festival, HttpContext context, IAntiforgery csrf, ChoirEnrollments enrollments) =>
        {
            if (group is <= 0 or > uint.MaxValue || festival is <= 0 or > uint.MaxValue ||
                context.User.FindFirstValue("actor_type") != "choir_person" ||
                !uint.TryParse(context.User.FindFirstValue(ClaimTypes.NameIdentifier), out var actor)) return Results.Forbid();
            if (!await csrf.IsRequestValidAsync(context)) return Results.BadRequest();
            var result = await enrollments.QueueNoteCopyAsync(actor, (uint)group, (uint)festival, context.RequestAborted);
            return Results.LocalRedirect($"/koor/{group}/deelnamehistorie?nota={result}");
        }).RequireAuthorization().RequireRateLimiting("login");
    }

    public static byte[] Render(ParticipationNote note, bool test, string? language = null)
    {
        var locale = language ?? note.MailLanguage;
        string T(string nl, string de, string en) => locale switch { "de" => de, "en" => en, _ => nl };
        using var document = new PdfDocument();
        var page = document.Pages.Add();
        var graphics = page.Graphics;
        var regular = new PdfStandardFont(PdfFontFamily.Helvetica, 11);
        var bold = new PdfStandardFont(PdfFontFamily.Helvetica, 11, PdfFontStyle.Bold);
        var heading = new PdfStandardFont(PdfFontFamily.Helvetica, 22, PdfFontStyle.Bold);
        using var currencyFontStream = typeof(ParticipationNotePdf).Assembly.GetManifestResourceStream("Amusing.Mijn.OpenSans-Regular.ttf")
            ?? throw new InvalidOperationException("PDF font resource is missing.");
        var currencyFont = new PdfTrueTypeFont(currencyFontStream, 11);
        var culture = CultureInfo.GetCultureInfo(T("nl-NL", "de-DE", "en-IE"));
        string Money(decimal amount) => amount.ToString("N2", culture);
        string Date(DateOnly date) => date.ToString(T("dd-MM-yyyy", "dd.MM.yyyy", "d MMM yyyy"), culture);

        const float width = 465;
        var currencyX = width - 96;
        graphics.DrawString("Korenfestival Amusing Hengelo", bold, PdfBrushes.Black, new PointF(0, 10));
        var title = test ? T("Testnota deelnamekosten", "Test-Teilnahmerechnung", "Test participation invoice")
            : T("Deelnamekosten", "Teilnahmerechnung", "Participation invoice");
        graphics.DrawString(title, heading, PdfBrushes.Black, new PointF(0, 32));
        if (note.IsCopy)
            graphics.DrawString(T("Kopie", "Kopie", "Copy"), bold, PdfBrushes.Black, new PointF(0, 56));
        graphics.DrawString($"{T("Kenmerk", "Referenz", "Reference")} {note.Reference}  |  {T("Uitgegeven op", "Ausgestellt am", "Issued on")} {Date(note.IssuedOn)}",
            regular, PdfBrushes.Black, new PointF(0, 72));

        graphics.DrawString($"{T("Koor", "Chor", "Choir")}: {note.ChoirName}", bold, PdfBrushes.Black, new RectangleF(0, 102, width, 52));
        graphics.DrawString($"{T("Festivaldatum", "Festivaldatum", "Festival date")}: {Date(note.FestivalDate)}", regular, PdfBrushes.Black, new PointF(0, 158));

        Row(199, T("Omschrijving", "Beschreibung", "Description"), T("Bedrag", "Betrag", "Amount"), bold);
        Line(217);
        var excludingVat = T("excl. btw", "zzgl. MwSt.", "excl. VAT");
        AmountRow(225, $"{T("Deelnemers", "Teilnehmer", "Participants")} ({note.Headcount}) {excludingVat}", note.Amount.DeelnemersExclBtw, regular);
        AmountRow(244, $"{T("Podiumtype", "Bühnentyp", "Stage type")} {note.Stage} {excludingVat}", note.Amount.PodiumExclBtw, regular);
        AmountRow(263, $"{T("Btw", "MwSt.", "VAT")} ({note.Amount.BtwPercentage.ToString("0.##", culture)}%)", note.Amount.Btw, regular);
        Line(282);
        AmountRow(290, T("Totaal incl. btw", "Gesamt inkl. MwSt.", "Total incl. VAT"), note.Amount.TotaalInclBtw, bold);
        graphics.DrawLine(PdfPens.Black, currencyX, 306, width, 306);
        graphics.DrawLine(PdfPens.Black, currencyX, 309, width, 309);

        var payment = test ? T("Dit is een testnota. Maak geen bedrag over.",
                "Dies ist eine Testrechnung. Bitte überweisen Sie kein Geld.", "This is a test invoice. Please do not make a payment.")
            : note.Paid ? T("Deze deelname is betaald.", "Die Teilnahmegebühr wurde bezahlt.", "This participation fee has been paid.")
            : T($"Maak het bedrag over naar NL22RABO0125893922\nten name van Stichting Vocale Promotie in Hengelo onder vermelding van {note.Reference}.\nHeb je al betaald? Dan hoef je niet opnieuw te betalen.",
                $"Bitte überweisen Sie den Betrag an Stichting Vocale Promotie in Hengelo.\nIBAN: NL22RABO0125893922. Verwendungszweck: {note.Reference}.\nWenn Sie bereits bezahlt haben, brauchen Sie nicht erneut zu zahlen.",
                $"Please transfer the amount to Stichting Vocale Promotie in Hengelo.\nIBAN: NL22RABO0125893922. Payment reference: {note.Reference}.\nIf you have already paid, you do not need to pay again.");
        graphics.DrawString(payment, regular, PdfBrushes.Black, new RectangleF(0, 340, width, 80));
        var footer = note.LayoutText ?? (note.Paid ? "" : note.PaymentTerm(locale));
        if (!string.IsNullOrWhiteSpace(footer))
        {
            var element = new Syncfusion.Pdf.Graphics.PdfTextElement(footer, regular);
            element.Draw(page, new RectangleF(0,430,width,graphics.ClientSize.Height - 440),
                new Syncfusion.Pdf.Graphics.PdfLayoutFormat { Layout = Syncfusion.Pdf.Graphics.PdfLayoutType.Paginate });
        }

        using var output = new MemoryStream();
        document.Save(output);
        return output.ToArray();

        void Line(float y) => graphics.DrawLine(PdfPens.Black, 0, y, width, y);
        void AmountRow(float y, string description, decimal amount, PdfFont font)
        {
            Row(y, description, Money(amount), font);
            graphics.DrawString("€", currencyFont, PdfBrushes.Black, new PointF(currencyX, y));
        }
        void Row(float y, string description, string value, PdfFont font)
        {
            graphics.DrawString(description, font, PdfBrushes.Black, new PointF(0, y));
            graphics.DrawString(value, font, PdfBrushes.Black, new PointF(width - font.MeasureString(value).Width, y));
        }
    }
}
