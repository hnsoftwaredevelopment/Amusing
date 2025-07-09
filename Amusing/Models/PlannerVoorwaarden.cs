namespace Amusing.Models;

public partial class PlannerVoorwaarden
{
    public uint FestivalId { get; set; }
    public sbyte WensTijdTussenOptredens { get; set; }
    public sbyte MaxTijdTussenOptredens { get; set; }
    public sbyte MaxLengteVrijwilligerDienst { get; set; }
    public sbyte BoeteOnderbrekingOptredens { get; set; }

    public virtual AhFestival Festival { get; set; } = null!;
}
