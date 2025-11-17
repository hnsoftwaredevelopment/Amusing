namespace Amusing.Models;

public class DashboardStatisticsStage
{
    public string Stagetype { get; set; } = string.Empty;
    public int Total { get; set; } = 0;
    public int InQueue { get; set; } = 0;
    public int Paid { get; set; } = 0;
    public int DroppedOut { get; set; } = 0;
}
