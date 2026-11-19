using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using Amusing.Facturatie;

namespace Amusing.Services;

public sealed record MijnTemplateVariable(string Name, string Explanation);

public static class MijnTemplateReference
{
    private static readonly string[] Common = ["Volledige naam","Groepsnaam","Editie","Festivaldatum","Aantal zangers","Podiumtype","Inschrijflink"];
    private static readonly string[] Note = ["Volledige naam","VolledigenaamNotaOntvanger","Groepsnaam","Koornaam","Editie","Festivaldatum",
        "Aantal zangers","Podiumtype","TotaalbedragNota","Datum inschrijving","EindeInschrijvingFestival","UitersteBetaaldatum","Betaaltermijn","Kenmerk"];

    public static IReadOnlyList<MijnTemplateVariable> ForKind(string kind)
    {
        IEnumerable<string> names = kind switch
        {
            "aanmelding" or "plaatsing" => Common.Concat(["Notamelding","RolNotaOntvanger","MailNotaOntvanger"]),
            "wachtlijst" => Common,
            "organisatie" => ["Groepsnaam","Editie","Inschrijfstatus","Aantal zangers","Podiumtype","MailBevestigingOntvanger",
                "RolBevestigingOntvanger","MailNotaOntvanger","RolNotaOntvanger"],
            "notamail" or "kopienota" or "nota_pdf" => Note,
            _ => []
        };
        return names.Select(name => new MijnTemplateVariable(name,Explain(name,kind))).ToArray();
    }

    private static string Explain(string name,string kind) => name switch
    {
        "Volledige naam" => kind is "aanmelding" or "wachtlijst" or "plaatsing"
            ? "Naam van contactpersoon 1, die deze bevestiging ontvangt. Als de naam ontbreekt wordt de koornaam gebruikt."
            : "Naam van de notaontvanger: penningmeester of contactpersoon 1. Ontbreekt de naam, dan wordt de koornaam gebruikt.",
        "VolledigenaamNotaOntvanger" => "Volledige naam van de notaontvanger. Dezelfde waarde als {Volledige naam} in de notamail.",
        "Groepsnaam" or "Koornaam" => "Naam van het ingeschreven koor. Deze twee variabelen betekenen bij nota’s hetzelfde.",
        "Editie" => "Jaar van de festivaldatum, bijvoorbeeld 2027.",
        "Festivaldatum" => "Datum waarop het festival plaatsvindt, geschreven in de taal van de mail.",
        "Aantal zangers" => "Het opgeslagen aantal deelnemers van deze inschrijving, inclusief deelnemers die geen zanger zijn.",
        "Podiumtype" => "Letter van het opgeslagen podiumtype, bijvoorbeeld B. De omschrijving en prijs worden hiermee niet getoond.",
        "Inschrijflink" => "Link naar de inschrijving van dit koor op de gebruikte Mijn-omgeving. Inloggen blijft nodig.",
        "Notamelding" => "Een complete vertaalde zin over de afzonderlijk verzonden nota, met rol en e-mailadres. Leeg als er geen nota is klaargezet. Gebruik voor eigen bewoordingen {RolNotaOntvanger} en {MailNotaOntvanger}.",
        "RolNotaOntvanger" => kind == "organisatie"
            ? "Penningmeester, of contactpersoon 1 met de reden dat geen bruikbare penningmeester is ingevuld. Bij een wachtlijstinschrijving is dit leeg."
            : "Alleen de rolnaam van de notaontvanger in de taal van de mail. Leeg als er geen nota is klaargezet.",
        "MailNotaOntvanger" => kind == "organisatie"
            ? "Het bedoelde nota-adres. Bij een wachtlijstinschrijving staat hier dat er geen nota wordt verstuurd. Op dev kan de echte testontvanger afwijken."
            : "Het bedoelde e-mailadres van de penningmeester, met contactpersoon 1 als terugval. Leeg als er geen nota is klaargezet. Op dev gaat de testmail naar de ingelogde tester.",
        "Inschrijfstatus" => "Een complete Nederlandse zin die zegt dat het koor is ingeschreven of op de wachtlijst is aangemeld.",
        "MailBevestigingOntvanger" => "Het bedoelde e-mailadres van contactpersoon 1. Als dat ontbreekt wordt dat vermeld. Op dev gaat de testmail naar de tester.",
        "RolBevestigingOntvanger" => "De vaste rol contactpersoon 1.",
        "TotaalbedragNota" => "Totale deelnamekosten inclusief btw, met twee decimalen en zonder euroteken. Plaats zelf € in de tekst. Dit is geen berekend restbedrag na eerdere betalingen.",
        "Datum inschrijving" => "Oorspronkelijke datum van inschrijving. Een wijziging van de inschrijving verschuift deze datum niet.",
        "EindeInschrijvingFestival" => "Sluitingsdatum van de inschrijfperiode van de editie.",
        "UitersteBetaaldatum" => "Alleen de uiterste betaaldatum. Normaal de vroegste van zes weken na inschrijving en de sluitingsdatum. Bij wachtlijstplaatsing rekenen we vanaf plaatsing. Na sluiting kiest Beheer de datum. Leeg als benodigde historische datums ontbreken.",
        "Betaaltermijn" => "Een complete betaalzin met de berekende datum. In het Nederlands wordt vlak voor de vervaldatum ‘Voldoe de nota voor …’ gebruikt, anders ‘Voldoe de nota uiterlijk op …’. Bij kopieën kan deze datum in het verleden liggen.",
        "Kenmerk" => "Betalingskenmerk AM-festivalnummer-koornummer. Het festivalnummer is een database-ID en kan afwijken van het editiejaar.",
        _ => ""
    };

    public static IReadOnlyDictionary<string,string> Examples(string kind,string language,string scenario)
    {
        language = kind == "organisatie" ? "nl" : language;
        string T(string nl,string de,string en) => language switch { "de" => de,"en" => en,_ => nl };
        var culture=CultureInfo.GetCultureInfo(T("nl-NL","de-DE","en-GB"));
        var nearClosing=scenario == "vlakvoor";
        var waitlisted=kind == "wachtlijst" || kind == "organisatie" && scenario == "wachtlijst";
        var noNote=waitlisted || scenario == "geennota";
        var primary=scenario == "contactpersoon1";
        var registration=nearClosing ? new DateOnly(2027,3,31) : new DateOnly(2027,1,15);
        var closes=new DateOnly(2027,4,1);
        var noteAddress=primary ? "contact@example.invalid" : "penningmeester@example.invalid";
        var role=primary ? T("Eerste contactpersoon","Erste Kontaktperson","Primary contact") : T("Penningmeester","Schatzmeister","Treasurer");
        var noteName=primary ? "Anna Voorbeeld" : "Robin Voorbeeld";
        var noteMessage=primary
            ? T($"De nota voor deze inschrijving is separaat naar u verstuurd ({noteAddress}).",$"Die Rechnung für diese Anmeldung wurde separat an Sie geschickt ({noteAddress}).",$"The invoice for this registration has been sent to you separately ({noteAddress}).")
            : T($"De nota voor deze inschrijving is separaat verstuurd naar de penningmeester ({noteAddress}).",$"Die Rechnung für diese Anmeldung wurde separat an den Schatzmeister geschickt ({noteAddress}).",$"The invoice for this registration has been sent separately to the treasurer ({noteAddress}).");
        string Date(DateOnly date) => date.ToString("d MMMM yyyy",culture);
        var values = new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Volledige naam"] = kind is "aanmelding" or "wachtlijst" or "plaatsing" ? "Anna Voorbeeld" : noteName,
            ["VolledigenaamNotaOntvanger"] = noteName,
            ["Groepsnaam"] = "Voorbeeldkoor",["Koornaam"] = "Voorbeeldkoor",["Editie"] = "2027",["Festivaldatum"] = Date(new(2027,6,5)),
            ["Aantal zangers"] = "25",["Podiumtype"] = "B",["Inschrijflink"] = "https://mijn.amusing-hengelo.nl/koor/123/inschrijven",
            ["Notamelding"] = noNote ? "" : noteMessage,
            ["RolNotaOntvanger"] = noNote ? "" : kind == "organisatie" ? primary ? "contactpersoon 1, want penningmeester niet ingevuld" : "penningmeester" : role,
            ["MailNotaOntvanger"] = noNote ? kind == "organisatie" ? waitlisted ? "Geen nota bij een wachtlijstinschrijving." : "Geen nota verstuurd." : "" : noteAddress,
            ["Inschrijfstatus"] = waitlisted ? "Zojuist heeft Voorbeeldkoor zich aangemeld voor de wachtlijst van het komende festival." : "Zojuist heeft Voorbeeldkoor zich ingeschreven voor het komende festival.",
            ["MailBevestigingOntvanger"] = "contact@example.invalid",["RolBevestigingOntvanger"] = "contactpersoon 1",
            ["TotaalbedragNota"] = 100m.ToString("N2",culture),["Datum inschrijving"] = Date(registration),["EindeInschrijvingFestival"] = Date(closes),
            ["UitersteBetaaldatum"] = Date(NotaBetaaltermijn.UitersteDatum(registration,closes)!.Value),
            ["Betaaltermijn"] = NotaBetaaltermijn.Tekst(registration,closes,registration,language),["Kenmerk"] = "AM-23-123"
        };
        return ForKind(kind).ToDictionary(variable => variable.Name,variable => values[variable.Name],StringComparer.OrdinalIgnoreCase);
    }

    public static string Expand(string template,IReadOnlyDictionary<string,string> values,bool html) =>
        Regex.Replace(template,@"\{([A-Za-z][A-Za-z0-9 ]*)\}",match => values.TryGetValue(match.Groups[1].Value.Trim(),out var value)
            ? html ? WebUtility.HtmlEncode(value) : value : match.Value);

    public static IReadOnlyList<string> Unknown(string template,IReadOnlyDictionary<string,string> values) =>
        Regex.Matches(template,@"\{([A-Za-z][A-Za-z0-9 ]*)\}").Select(match => match.Groups[1].Value.Trim())
            .Where(name => !values.ContainsKey(name)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
}
