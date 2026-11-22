using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;

namespace Amusing.Mijn.Services;

internal static class EnrollmentConfirmationMail
{
    internal static (string Subject, string Body, string? Html) Create(string language, DateOnly date, string choir,
        FestivalRegistration registration, string url, string? noteRole,
        string? subjectTemplate = null, string? bodyTemplate = null, string? recipientName = null, string? htmlTemplate = null,
        string? noteRecipient = null, bool placement = false)
    {
        string T(string nl, string de, string en) => language switch { "de" => de, "en" => en, _ => nl };
        var culture = CultureInfo.GetCultureInfo(T("nl-NL", "de-DE", "en-GB"));
        var festival = $"Korenfestival Amusing Hengelo {date.Year}";
        var title = registration.Waitlisted
            ? T("Wachtlijst", "Warteliste", "Waiting list")
            : T("Aanmelding Amusing", "Anmeldung Amusing", "Amusing registration");
        if (placement) title = T("Plaatsing Amusing","Teilnahme bestätigt Amusing","Amusing place confirmed");
        var greeting = registration.Waitlisted
            ? T($"We hebben de aanmelding van {choir} voor {festival} ontvangen. Het koor staat op de wachtlijst. We laten jullie weten of er een plek vrijkomt.",
                $"Wir haben die Anmeldung von {choir} für das {festival} erhalten. Der Chor steht auf der Warteliste. Wir melden uns, sobald ein Platz frei wird.",
                $"We have received the registration of {choir} for {festival}. Your choir is on the waiting list. We will let you know if a place opens up.")
            : T($"{choir} is ingeschreven voor {festival}. Fijn dat jullie meedoen!",
                $"{choir} ist für das {festival} angemeldet. Schön, dass ihr dabei seid!",
                $"{choir} has registered for {festival}. We’re glad you’re taking part!");
        if (placement) greeting = T($"{choir} is vanaf de wachtlijst geplaatst voor {festival}.",
            $"{choir} wurde von der Warteliste für das {festival} zugelassen.",
            $"{choir} has moved from the waiting list to participate in {festival}.");
        var noteMessage = noteRole switch
        {
            "penningmeester" => T("De nota voor deze inschrijving is separaat verstuurd naar de penningmeester.",
                "Die Rechnung für diese Anmeldung wurde separat an den Schatzmeister geschickt.",
                "The invoice for this registration has been sent separately to the treasurer."),
            "contactpersoon1" => T("De nota voor deze inschrijving is separaat naar u verstuurd.",
                "Die Rechnung für diese Anmeldung wurde separat an Sie geschickt.",
                "The invoice for this registration has been sent to you separately."),
            _ => ""
        };
        if (!string.IsNullOrWhiteSpace(noteRecipient) && noteMessage.Length > 0)
            noteMessage = noteMessage.TrimEnd('.') + $" ({noteRecipient}).";
        if (subjectTemplate is not null && bodyTemplate is not null)
        {
            var subjectText = Expand(subjectTemplate).Replace('\r', ' ').Replace('\n', ' ').Trim();
            return (subjectText[..Math.Min(subjectText.Length, 240)], Expand(bodyTemplate),
                htmlTemplate is null ? null : Expand(htmlTemplate, html: true));
        }

        string Expand(string template, bool html = false) => Regex.Replace(template, @"\{([^{}]+)\}|\[([^\[\]]+)\]", match =>
        {
            var value = (match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value).Trim().ToLowerInvariant() switch
            {
                "volledige naam" or "name" => string.IsNullOrWhiteSpace(recipientName) ? choir : recipientName,
                "groepsnaam" or "groupname" or "koor" => choir,
                "editie" or "festival" or "jaar" => date.Year.ToString(CultureInfo.InvariantCulture),
                "festivaldatum" => date.ToString("d MMMM yyyy", culture),
                "aantal zangers" or "singers" or "deelnemers" => registration.Headcount.ToString(culture),
                "podiumtype" or "stagetype" => registration.PodiumType,
                "inschrijflink" => url,
                "notamelding" => noteMessage,
                "rolnotaontvanger" => noteRole is null ? "" : ChoirRoleLabels.Get(noteRole,language),
                "mailnotaontvanger" => noteRecipient ?? "",
                _ => null
            };
            return value is null ? match.Value : html ? WebUtility.HtmlEncode(value) : value;
        });
        var paragraphs = new[]
        {
            T("Hallo,", "Hallo,", "Hello,"),
            greeting,
            $"""
            {T("Koor", "Chor", "Choir")}: {choir}
            {T("Festivaldatum", "Festivaldatum", "Festival date")}: {date.ToString("d MMMM yyyy", culture)}
            {T("Aantal deelnemers", "Teilnehmerzahl", "Number of participants")}: {registration.Headcount}
            {T("Podiumtype", "Bühnentyp", "Stage type")}: {registration.PodiumType}
            """,
            noteMessage,
            T("Bekijk je inschrijving op", "Hier könnt ihr eure Anmeldung ansehen:", "View your registration at") + " " + url,
            $"{T("Met vriendelijke groeten,", "Viele Grüße,", "Kind regards,")}\nKorenfestival Amusing Hengelo"
        };
        var subject = $"{title} {date.Year} – {choir.Replace('\r', ' ').Replace('\n', ' ')}";
        return (subject, string.Join("\n\n", paragraphs.Where(paragraph => paragraph.Length != 0)), null);
    }
}
