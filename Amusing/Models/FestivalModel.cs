using System.ComponentModel.DataAnnotations;

namespace Amusing.Models;

public class FestivalModel
{
    public uint FestivalId { get; set; }

    [Display( Name = "Editie" )]
    public string Festival { get; set; } = string.Empty;

    [Display( Name = "Gepubliceerd" )]
    public string Gepubliceerd { get; set; } = string.Empty;

    [Display( Name = "Actief" )]
    public int Aktief { get; set; } = 0;

    [Display( Name = "Festivaldatum" )]
    public DateOnly Festivaldatum { get; set; }

    [Display( Name = "Start inschrijving" )]
    public DateOnly StartInschrijving { get; set; }

    [Display( Name = "Einde inschrijving" )]
    public DateOnly EindeInschrijving { get; set; }

    [Display( Name = "Wachtlijst" )]
    public int Wachtlijst { get; set; }

    [Display( Name = "Publiceer planning" )]
    public int PubliceerPlanning { get; set; }

    [Display( Name = "Start vrijwilligerstaken" )]
    public TimeOnly StartVrijwilligersTaken { get; set; }

    [Display( Name = "Einde vrijwilligerstaken" )]
    public TimeOnly EindeVrijwilligersTaken { get; set; }

    [Display( Name = "Start vrijwilligerspauze" )]
    public TimeOnly StartVrijwilligersPauze { get; set; }

    [Display( Name = "Einde vrijwilligerspauze" )]
    public TimeOnly EindeVrijwilligersPauze { get; set; }

    [Display( Name = "Einde vaste vrijwilligerstaken" )]
    public TimeOnly EindeVasteVrijwilligersTaken { get; set; }


    // additional data From planner_voorwaarden
    [Display( Name = "Aantal minuten tussen de optredens" )]
    public int MinutenTussenOptredens { get; set; }

    [Display( Name = "Maximaal aantal minuten tussen optredens" )]
    public int MaximumMinutenTussenOptredens { get; set; }

    [Display( Name = "Maximaal aantal uren voor vrijwilligers" )]
    public int MaximumUrenVrijwilligers { get; set; }

    [Display( Name = "Boete bij onderbreking optredens" )]
    public decimal BoeteOnderbrekingOptredens { get; set; }

    // --- Export-safe mirror properties ---
    [Display( Name = "(Technisch veld) Festivaldatum voor export functionaliteit")]
    public DateTime FestivaldatumExport => Festivaldatum.ToDateTime( TimeOnly.MinValue );

    [Display( Name = "(Technisch veld) Start inschrijving voor export functionaliteit" )]
    public DateTime StartInschrijvingExport => StartInschrijving.ToDateTime( TimeOnly.MinValue );

    [Display( Name = "(Technisch veld) Einde inschrijving voor export functionaliteit" )]
    public DateTime EindeInschrijvingExport => EindeInschrijving.ToDateTime( TimeOnly.MinValue );

    [Display( Name = "(Technisch veld) Start vrijwilligers taken voor export functionaliteit" )]
    public DateTime StartVrijwilligersTakenExport => DateTime.Today.Add( StartVrijwilligersTaken.ToTimeSpan() );

    [Display( Name = "(Technisch veld) Einde vrijwilligers taken voor export functionaliteit" )]
    public DateTime EindeVrijwilligersTakenExport => DateTime.Today.Add( EindeVrijwilligersTaken.ToTimeSpan() );

    [Display( Name = "(Technisch veld) Start vrijwilligers pauze voor export functionaliteit" )]
    public DateTime StartVrijwilligersPauzeExport => DateTime.Today.Add( StartVrijwilligersPauze.ToTimeSpan() );

    [Display( Name = "(Technisch veld) Einde vrijwilligers pauze voor export functionaliteit" )]
    public DateTime EindeVrijwilligersPauzeExport => DateTime.Today.Add( EindeVrijwilligersPauze.ToTimeSpan() );
    
    [Display( Name = "(Technisch veld) Einde vaste vrijwilligers taken voor export functionaliteit" )]
    public DateTime EindeVasteVrijwilligersTakenExport => DateTime.Today.Add( EindeVasteVrijwilligersTaken.ToTimeSpan() );
}
