namespace Amusing.Models;

public partial class PlannerVrijwilligersdiensten
{
    public uint FestivalId { get; set; }
    public uint PersoonId { get; set; }
    public int PodiumId { get; set; }
    public TimeOnly Van { get; set; }
    public TimeOnly Tot { get; set; }
    public uint? Taak { get; set; }
    public string Vastgezet { get; set; }

    public virtual AhFestival Festival { get; set; } = null!;
    public virtual AhPersonen Persoon { get; set; } = null!;
    public virtual AhTaken TaakNavigation { get; set; }
}
