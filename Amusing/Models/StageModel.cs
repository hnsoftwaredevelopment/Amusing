namespace Amusing.Models;

public class StageModel
{
    public uint PodiumId { get; set; }
    public string Naam { get; set; } = string.Empty;
    public string Soort { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int Kwaliteit { get; set; }
    public int MaxZangers { get; set; }
    public int Vrijwilligers { get; set; } = 0;
    public TimeOnly Start { get; set; }
    public TimeOnly Eind { get; set; }
    public TimeOnly Van { get; set; }
    public TimeOnly Tot { get; set; }
    public int KaartNummer { get; set; }
    public int Aktief { get; set; } = 0;

}
