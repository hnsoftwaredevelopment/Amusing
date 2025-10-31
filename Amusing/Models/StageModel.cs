using System.ComponentModel.DataAnnotations;

namespace Amusing.Models;

public class StageModel
{
    public uint PodiumId { get; set; }

    [Display( Name = "Podium naam" )]
    public string Naam { get; set; } = string.Empty;

    [Display( Name = "Podium soort" )]
    public string Soort { get; set; } = string.Empty;

    [Display( Name = "Podium type" )]
    public string Type { get; set; } = string.Empty;


    [Display( Name = "NFVE" )]
    public string Nfve { get; set; } = string.Empty;

    [Display( Name = "Kwaliteit" )]
    public int Kwaliteit { get; set; }

    [Display( Name = "Maximaal aantal zangers" )]
    public int MaxZangers { get; set; }

    [Display( Name = "Aantal vrijwilligers" )]
    public int Vrijwilligers { get; set; } = 0;

    [Display( Name = "Start tijd" )]
    public TimeOnly Start { get; set; }

    [Display( Name = "Eind tijd" )]
    public TimeOnly Eind { get; set; }

    [Display( Name = "Vrijwilligers aanwezig van" )]
    public TimeOnly Van { get; set; }

    [Display( Name = "Vrijwilligers aanwezig tot" )]
    public TimeOnly Tot { get; set; }
    
    [Display( Name = "Podiumnummer op de kaart" )]
    public int KaartNummer { get; set; }

    [Display( Name = "Podium aktief" )]
    public int Aktief { get; set; } = 0;

    // --- Export-safe mirror properties ---
    [Display( Name = "(Technisch veld) Start tijd voor export functionaliteit" )]
    public DateTime StartExport => DateTime.Today.Add( Start.ToTimeSpan() );


    [Display( Name = "(Technisch veld) Eind tijd voor export functionaliteit" )]
    public DateTime EindExport => DateTime.Today.Add( Eind.ToTimeSpan() );

    [Display( Name = "(Technisch veld) Vrijwilligers aanwezig van, voor export functionaliteit" )]
    public DateTime VanExport => DateTime.Today.Add( Van.ToTimeSpan() );


    [Display( Name = "(Technisch veld) Vrijwilligers aanwezig tot, voor export functionaliteit" )]
    public DateTime TotExport => DateTime.Today.Add( Tot.ToTimeSpan() );
}
