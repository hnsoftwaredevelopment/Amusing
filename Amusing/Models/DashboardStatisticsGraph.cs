namespace Amusing.Models;

public class DashboardStatisticsGraph
{
    public int FestivalId { get; set; }
    public string Festival { get; set; } = string.Empty;
    public string Month { get; set; } = string.Empty;
    public int MonthOrder { get; set; }
    public int Number { get; set; }
    public string Type { get; set; }
}
