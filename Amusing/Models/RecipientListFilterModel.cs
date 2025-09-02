namespace Amusing.Models;

public class RecipientListFilterModel
{
    public int PersonId { get; set; }
    public string Firstname { get; set; } = string.Empty;
    public string Infix { get; set; } = string.Empty;
    public string Lastname { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool Infomailing { get; set; }
    public bool Active { get; set; }
    public string Role { get; set; } = string.Empty;
    public int GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public int FestivalId { get; set; }
    public int Festival { get; set; }
    public string StageType { get; set; } = string.Empty;
    public bool Subscribed { get; set; }
    public bool Canceled { get; set; }
    public bool Payed { get; set; }
    public bool Confirmed { get; set; }
    public int Singers { get; set; }
}
