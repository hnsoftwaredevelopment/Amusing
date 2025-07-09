namespace Amusing.Models;

public partial class AhPodiaTypen
{
    public string Type { get; set; } = null!;
    public byte Prijs { get; set; }
    public byte Piano { get; set; }
    public byte Electra { get; set; }
    public byte Drum { get; set; }
    public byte Gitaarversterkers { get; set; }
    public byte Microfoons { get; set; }
    public string Beschrijving { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string CompatibelMet { get; set; } = null!;
    /// <summary>
    /// oudste festival_id waarop podiumtype geldig was
    /// </summary>
    public int Versie { get; set; }
}
