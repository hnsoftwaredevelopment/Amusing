namespace Amusing.Models;

public class StageModel
{
    public int PodiumId { get; set; }
    public string? Naam { get; set; } = string.Empty;
    public string? Soort { get; set; } = string.Empty;
    public string? Type { get; set; } = string.Empty;
    public int Kwaliteit { get; set; }
    public int MaxZangers { get; set; }
    public string? Vrijwilligers { get; set; } = string.Empty;
    public TimeOnly Start { get; set; }
    public TimeOnly Eind { get; set; }
    public TimeOnly Van { get; set; }
    public TimeOnly Tot { get; set; }
    public int KaartId { get; set; }

}
