namespace Amusing.Models;

/// <summary>
/// autoinc was 4049
/// </summary>
public partial class AhProfielbeheerLog
{
    public uint LogId { get; set; }
    public DateTime? Date { get; set; }
    public string Action { get; set; } = null!;
    public uint ZanggroepId { get; set; }
    public string IpAddress { get; set; } = null!;
    public string Report { get; set; }
}
