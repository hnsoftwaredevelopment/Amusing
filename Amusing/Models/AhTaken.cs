namespace Amusing.Models;

public partial class AhTaken
{
    public AhTaken()
    {
        PlannerVrijwilligersdienstens = new HashSet<PlannerVrijwilligersdiensten>();
    }

    public uint TaakId { get; set; }
    public string KorteNaam { get; set; } = null!;
    public string Naam { get; set; } = null!;
    public uint Minimumduur { get; set; }
    public uint Maximumduur { get; set; }
    public string Bezetting { get; set; } = null!;
    public string Actief { get; set; }
    public string Omschrijving { get; set; } = null!;

    public virtual ICollection<PlannerVrijwilligersdiensten> PlannerVrijwilligersdienstens { get; set; }
}
