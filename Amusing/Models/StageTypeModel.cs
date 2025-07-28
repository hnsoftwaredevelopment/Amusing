namespace Amusing.Models;

public class StageTypeModel
{
    public string Type { get; set; } = string.Empty;
    public string Price { get; set; } = string.Empty;
    public int Piano { get; set; } = 0;
    public int Lectern { get; set; } = 0;
    public int Electronics { get; set; } = 0;
    public int Drums { get; set; } = 0;
    public int GitarEmplifiers { get; set; } = 0;
    public int BassEmplifiers { get; set; } = 0;
    public int ChoirEmplifiers { get; set; } = 0;
    public int Microphones { get; set; } = 0;
    public int Monitors { get; set; } = 0;
    public int Speakers { get; set; } = 0;
    public int MixingConsole { get; set; } = 0;
    public int Mp3 { get; set; } = 0;
    public string Description { get; set; } = string.Empty;
    public string Compatibel { get; set; } = string.Empty;
    public int Active { get; set; } = 0;
    public bool IsActive => Active == 1;
}
