namespace Amusing.Models;

public partial class AhWenssoorten
{
    public uint WenssoortId { get; set; }
    public string KortNl { get; set; } = null!;
    public string KortDe { get; set; } = null!;
    public string KortEn { get; set; } = null!;
    public string LangNl { get; set; } = null!;
    public string LangDe { get; set; } = null!;
    public string LangEn { get; set; } = null!;
    public sbyte Zichtbaar { get; set; }
}
