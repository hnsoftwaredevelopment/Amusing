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
}
