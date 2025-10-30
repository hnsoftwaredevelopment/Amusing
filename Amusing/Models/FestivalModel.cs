namespace Amusing.Models;

public class FestivalModel
{
    public uint FestivalId { get; set; }
    public string Festival { get; set; } = string.Empty;
    public string Gepubliceerd { get; set; } = string.Empty;
    public int Aktief { get; set; } = 0;
    public DateOnly Festivaldatum { get; set; }
    public DateOnly StartInschrijving { get; set; }
    public DateOnly EindeInschrijving { get; set; }
    public int Wachtlijst { get; set; }
    public int PubliceerPlanning { get; set; }
    public TimeOnly StartVrijwilligersTaken { get; set; }
    public TimeOnly EindeVrijwilligersTaken { get; set; }
    public TimeOnly StartVrijwilligersPauze { get; set; }
    public TimeOnly EindeVrijwilligersPauze { get; set; }
    public TimeOnly EindeVasteVrijwilligersTaken { get; set; }


    // additional data From planner_voorwaarden
    public int MinutenTussenOptredens { get; set; }
    public int MaximumMinutenTussenOptredens { get; set; }
    public int MaximumUrenVrijwilligers { get; set; }
    public decimal BoeteOnderbrekingOptredens { get; set; }

    // --- Export-safe mirror properties ---
    public DateTime FestivaldatumExport => Festivaldatum.ToDateTime( TimeOnly.MinValue );
    public DateTime StartInschrijvingExport => StartInschrijving.ToDateTime( TimeOnly.MinValue );
    public DateTime EindeInschrijvingExport => EindeInschrijving.ToDateTime( TimeOnly.MinValue );

    public DateTime StartVrijwilligersTakenExport => DateTime.Today.Add( StartVrijwilligersTaken.ToTimeSpan() );
    public DateTime EindeVrijwilligersTakenExport => DateTime.Today.Add( EindeVrijwilligersTaken.ToTimeSpan() );
    public DateTime StartVrijwilligersPauzeExport => DateTime.Today.Add( StartVrijwilligersPauze.ToTimeSpan() );
    public DateTime EindeVrijwilligersPauzeExport => DateTime.Today.Add( EindeVrijwilligersPauze.ToTimeSpan() );
    public DateTime EindeVasteVrijwilligersTakenExport => DateTime.Today.Add( EindeVasteVrijwilligersTaken.ToTimeSpan() );
}
