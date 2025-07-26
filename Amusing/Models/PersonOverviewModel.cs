namespace Amusing.Models;

public class PersonOverviewModel
{
    public uint PersoonId { get; set; }
    public string Naam { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> Rollen { get; set; } = [ ];
    public List<string> Vrijwilliger { get; set; } = [ ];
}
