namespace Amusing.Models;

public partial class AhFestival
{
    public AhFestival()
    {
        AhInschrijvingens = new HashSet<AhInschrijvingen>();
        AhVrijwilligers = new HashSet<AhVrijwilliger>();
        PlannerOptredens = new HashSet<PlannerOptreden>();
        PlannerVrijwilligersdienstens = new HashSet<PlannerVrijwilligersdiensten>();
    }

    public uint FestivalId { get; set; }
    public DateOnly Festivaldatum { get; set; }
    public DateTime StartInschrijving { get; set; }
    public DateTime EindInschrijving { get; set; }
    public byte Wachtlijst { get; set; }
    public sbyte PlanningPubliceren { get; set; }
    public TimeOnly StartFestivaldag { get; set; }
    public TimeOnly EindeFestivaldag { get; set; }
    public TimeOnly BeginPauze { get; set; }
    public TimeOnly EindePauze { get; set; }
    public TimeOnly EindeErvarenReserve { get; set; }

    public virtual PlannerVoorwaarden PlannerVoorwaarden { get; set; } = null!;
    public virtual ICollection<AhInschrijvingen> AhInschrijvingens { get; set; }
    public virtual ICollection<AhVrijwilliger> AhVrijwilligers { get; set; }
    public virtual ICollection<PlannerOptreden> PlannerOptredens { get; set; }
    public virtual ICollection<PlannerVrijwilligersdiensten> PlannerVrijwilligersdienstens { get; set; }
}
