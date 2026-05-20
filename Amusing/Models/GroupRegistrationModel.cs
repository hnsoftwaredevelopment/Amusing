namespace Amusing.Models;

public class GroupRegistrationModel
{
    public uint FestivalId { get; set; }
    public string Festival { get; set; } = string.Empty;
    public DateTime RegisteredAt { get; set; }
    public int Singers { get; set; }
    public string StageType { get; set; } = string.Empty;
    public string Paid { get; set; } = string.Empty;
    public string Confirmed { get; set; } = string.Empty;
    public string DroppedOut { get; set; } = string.Empty;
}
