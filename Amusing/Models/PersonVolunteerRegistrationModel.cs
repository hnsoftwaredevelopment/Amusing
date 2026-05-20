namespace Amusing.Models;

public class PersonVolunteerRegistrationModel
{
    public uint VolunteerId { get; set; }
    public uint FestivalId { get; set; }
    public string Festival { get; set; } = string.Empty;
    public DateTime SignedUpAt { get; set; }
    public string DroppedOut { get; set; } = string.Empty;
}
