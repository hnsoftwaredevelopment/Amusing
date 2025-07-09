namespace Amusing.Models;

/// <summary>
/// autoinc was 3048
/// </summary>
public partial class AhBeheerLog
{
    public uint LogId { get; set; }
    public DateTime? Date { get; set; }
    public string Action { get; set; } = null!;
    public uint UserId { get; set; }
    public string IpAddress { get; set; } = null!;
    public string Report { get; set; }
}
