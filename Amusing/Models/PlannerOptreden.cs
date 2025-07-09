namespace Amusing.Models;

public partial class PlannerOptreden
{
    public uint FestivalId { get; set; }
    public uint ZanggroepId { get; set; }
    public byte Tijdvak { get; set; }
    public uint PodiumId { get; set; }

    public virtual AhFestival Festival { get; set; } = null!;
    public virtual AhPodium Podium { get; set; } = null!;
    public virtual AhZanggroepen Zanggroep { get; set; } = null!;
}
