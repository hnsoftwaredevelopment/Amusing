using System.Globalization;

namespace Amusing.Facturatie;

public static class NotaBetaaltermijn
{
    public static DateOnly? UitersteDatum(DateOnly? ingeschreven, DateOnly? sluiting) =>
        ingeschreven is { } start && sluiting is { } end ? (start.AddDays(42) < end ? start.AddDays(42) : end) : null;

    public static string Tekst(DateOnly? ingeschreven, DateOnly? sluiting, DateOnly uitgegeven, string taal)
    {
        return TekstVoorDatum(UitersteDatum(ingeschreven,sluiting),uitgegeven,taal);
    }

    public static string TekstVoorDatum(DateOnly? uitersteDatum,DateOnly uitgegeven,string taal)
    {
        if (uitersteDatum is not { } due) return "";
        var culture = CultureInfo.GetCultureInfo(taal switch { "de" => "de-DE", "en" => "en-GB", _ => "nl-NL" });
        var date = due.ToString("d MMMM yyyy",culture);
        return taal switch
        {
            "de" => $"Bitte begleichen Sie die Rechnung innerhalb von 6 Wochen, spätestens jedoch bis zum {date}.",
            "en" => $"Please pay the invoice within 6 weeks; your payment deadline is {date}.",
            _ => due.DayNumber - uitgegeven.DayNumber <= 1 ? $"Voldoe de nota voor {date}." : $"Voldoe de nota binnen 6 weken; voor u is dit uiterlijk op {date}."
        };
    }
}
