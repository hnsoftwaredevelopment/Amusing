namespace Amusing.Models;

public partial class PersonModel
{
    public uint PersoonId { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public int Active { get; set; }
    public uint GroupId { get; set; }
    public string? Role { get; set; }
}
