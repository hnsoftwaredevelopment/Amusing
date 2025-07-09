namespace Amusing.Models;

/// <summary>
/// autoinc was 1566
/// </summary>
public partial class AhPersonen
{
    public AhPersonen()
    {
        AhPersonenRollens = new HashSet<AhPersonenRollen>();
        AhProfielens = new HashSet<AhProfielen>();
        AhVrijwilligers = new HashSet<AhVrijwilliger>();
        PlannerVrijwilligersdienstens = new HashSet<PlannerVrijwilligersdiensten>();
    }

    public uint PersoonId { get; set; }
    public string Voornaam { get; set; } = null!;
    public string Tussenvoegsel { get; set; }
    public string Achternaam { get; set; } = null!;
    public string Email { get; set; }
    public bool? Actief { get; set; }
    public byte Infomailing { get; set; }

    public virtual AhContactgegeven AhContactgegeven { get; set; } = null!;
    public virtual AhPersonenWachtwoorden AhPersonenWachtwoorden { get; set; } = null!;
    public virtual ICollection<AhPersonenRollen> AhPersonenRollens { get; set; }
    public virtual ICollection<AhProfielen> AhProfielens { get; set; }
    public virtual ICollection<AhVrijwilliger> AhVrijwilligers { get; set; }
    public virtual ICollection<PlannerVrijwilligersdiensten> PlannerVrijwilligersdienstens { get; set; }
}
