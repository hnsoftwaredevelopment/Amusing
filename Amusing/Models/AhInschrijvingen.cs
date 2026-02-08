namespace Amusing.Models;

public partial class AhInschrijvingen
{
    public uint FestivalId { get; set; }
    public uint ZanggroepId { get; set; }
    /// <summary>
    /// kleedruimte
    /// </summary>
    public string Wens1 { get; set; } = null!;
    /// <summary>
    /// singalong
    /// </summary>
    public string Wens2 { get; set; } = null!;
    /// <summary>
    /// Acapella Battle
    /// </summary>
    public string Wens3 { get; set; } = null!;
    /// <summary>
    /// beoordeling
    /// </summary>
    public string Wens4 { get; set; } = null!;
    public string Nfve { get; set; } = null!;
    public string Afactor { get; set; } = null!;
    public uint AantalDeelnemers { get; set; }
    public string Podiumsoort { get; set; } = null!;
    public byte PodiumkeuzeGeforceerd { get; set; }
    public DateTime Ingeschreven { get; set; }
    public DateTime? Betaald { get; set; }
    public DateOnly? Afgehaakt { get; set; }
    public TimeOnly BeschikbaarVan { get; set; }
    public TimeOnly BeschikbaarTot { get; set; }
    public byte Wachtlijst { get; set; }
    public byte Binnenoptredens { get; set; }
    public byte Buitenoptredens { get; set; }
    public DateTime? Bevestigd { get; set; }

    public virtual AhFestival Festival { get; set; } = null!;
    public virtual AhZanggroepen Zanggroep { get; set; } = null!;
}
