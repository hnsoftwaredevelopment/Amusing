namespace Amusing.Models;

public class FestivalModel
{
    public uint FestivalId { get; set; }
    public string Festival { get; set; } = string.Empty;
    public DateOnly Datum { get; set; }
    public string Gepubliceerd { get; set; } = string.Empty;
}
