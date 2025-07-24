namespace Amusing.Models;

public class FestivalParticipationDynamicViewModel
{
    public int ZanggroepId { get; set; }
    public string? Naam { get; set; }
    public string? Stad { get; set; }
    public string? Aangemaakt { get; set; }

    // Festival year → Registration date
    public Dictionary<int, string> DeelnamePerJaar { get; set; } = [ ];
}
