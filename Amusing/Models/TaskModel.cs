namespace Amusing.Models;

public class TaskModel
{
    public uint TaakId { get; set; }
    public string Naam { get; set; } = string.Empty;
    public string MinimumDuur { get; set; } = string.Empty;
    public string MaximumDuur { get; set; } = string.Empty;
    public string Van { get; set; } = string.Empty;
    public string Tot { get; set; } = string.Empty;
    public string Aantal { get; set; } = string.Empty;

    // For Task Maintenance
    public uint TaskId { get; set; }
    public string? ShortName { get; set; } = string.Empty;
    public string? Name { get; set; } = string.Empty;
    public int MinTimeSpan { get; set; }
    public int MaxTimeSpan { get; set; }
    public TimeOnly? TimeBlock1From { get; set; }
    public TimeOnly? TimeBlock1Until { get; set; }
    public uint TimeBlock1Volunteers { get; set; }
    public TimeOnly? TimeBlock2From { get; set; }
    public TimeOnly? TimeBlock2Until { get; set; }
    public uint TimeBlock2Volunteers { get; set; }
    public string? Description { get; set; } = string.Empty;
    public string? Active { get; set; } = string.Empty;
    public bool ActiveBool
    {
        get => Active?.ToLower() == "ja";
        set => Active = value ? "ja" : "nee";
    }
    public bool IsActive => Active == "ja";
}
