namespace Amusing.Models;

public partial class AhPodium
{
    public AhPodium()
    {
        PlannerOptredens = new HashSet<PlannerOptreden>();
    }

    public uint PodiumId { get; set; }
    public string Naam { get; set; } = null!;
    public string Soort { get; set; } = null!;
    public string Nfve { get; set; } = null!;
    public string Type { get; set; } = null!;
    public byte Kwaliteit { get; set; }
    public byte MaxZangers { get; set; }
    public string AantalVrijwilligers { get; set; } = null!;
    public TimeOnly Opening { get; set; }
    public TimeOnly Sluiting { get; set; }
    public TimeOnly VrijwilligersVanaf { get; set; }
    public TimeOnly VrijwilligersTot { get; set; }
    public byte? KaartNummer { get; set; }

    public virtual ICollection<PlannerOptreden> PlannerOptredens { get; set; }
}
