namespace Amusing.Models;

public partial class AhZanggroepen
{
    public AhZanggroepen()
    {
        AhInschrijvingens = new HashSet<AhInschrijvingen>();
        PlannerOptredens = new HashSet<PlannerOptreden>();
    }

    public uint ZanggroepId { get; set; }
    public string Naam { get; set; } = null!;
    public byte GenreId { get; set; }
    public string Standplaats { get; set; } = null!;
    public string Land { get; set; } = null!;
    public string Website { get; set; }
    public byte [ ] Foto { get; set; }
    public byte [ ] Logo { get; set; }
    public string Beschrijving { get; set; } = null!;
    public string Rekeningnr { get; set; }
    public bool? Actief { get; set; }

    public virtual AhProfielen AhProfielen { get; set; } = null!;
    public virtual AhZanggroepDetail AhZanggroepDetail { get; set; } = null!;
    public virtual ICollection<AhInschrijvingen> AhInschrijvingens { get; set; }
    public virtual ICollection<PlannerOptreden> PlannerOptredens { get; set; }
}
