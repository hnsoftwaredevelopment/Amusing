namespace Amusing.Models;

public class RegistrationModel
{
    public uint FestivalId { get; set; }
    public uint GroepId { get; set; }
    public DateTime Datum { get; set; }
    public string Naam { get; set; }
    public string Stad { get; set; }
    public string Podium { get; set; }
    public int Zangers { get; set; }
    public string Genre { get; set; }
    public decimal TeBetalen { get; set; }
    public string Betaald { get; set; }
    public DateTime? Betaaldatum { get; set; }
    public string Bevestigd { get; set; }
    public string Kleedkamer { get; set; }
    public string SingAlong { get; set; }
    public string AcapellaBattle { get; set; }
    public string Beoordeling { get; set; }
    public string Stand { get; set; }
    public int Binnen { get; set; }
    public int Buiten { get; set; }
    public string Afgehaakt { get; set; }
}
