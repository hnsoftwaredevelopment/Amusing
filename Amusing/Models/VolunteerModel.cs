namespace Amusing.Models;

public class VolunteerModel
{
    public uint FestivalId { get; set; }
    public DateTime Datum { get; set; }
    public string? Naam { get; set; }
    public string? Van { get; set; }
    public string? Tot { get; set; }
    public int Uren { get; set; }
    public string? Lunch { get; set; }
    public string? Vegetarisch { get; set; }
    public string? Bijeenkomst { get; set; }
    public string? Ervaring { get; set; }
    public string? Podiumdienst { get; set; }
    public string? Overige { get; set; }
    public string? Afgehaakt { get; set; }
}
