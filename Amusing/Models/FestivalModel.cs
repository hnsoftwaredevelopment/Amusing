namespace Amusing.Models;

public class FestivalModel
{
    public uint FestivalId { get; set; }
    public string Festival { get; set; } = string.Empty;
    public DateOnly Datum { get; set; }
    public string Gepubliceerd { get; set; } = string.Empty;


    // Additional data from ah_festivals
    public DateOnly Festivaldatum { get; set; }
    public DateOnly StartInschrijving { get; set; }
    public DateOnly EindeInschrijving { get; set; }
    public byte Wachtlijst { get; set; }
    public byte PubliceerPlanning { get; set; }
    public TimeOnly StartVrijwilligersTaken { get; set; }
    public TimeOnly EindeVrijwilligersTaken { get; set; }
    public TimeOnly StartVrijwilligersPauze { get; set; }
    public TimeOnly EindeVrijwilligersPauze { get; set; }
    public TimeOnly EindeVasteVrijwilligersTaken { get; set; }


    // additional data from planner_voorwaarden
    public int MinutenTussenOptredens { get; set; }
    public int MaximumMinutenTussenOptredens { get; set; }
    public int MaximumUrenVrijwilligers { get; set; }
    public decimal BoeteOnderbrekingOptredens { get; set; }
}
