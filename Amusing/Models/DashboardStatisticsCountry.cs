namespace Amusing.Models;

public class DashboardStatisticsCountry
{
    public string Country { get; set; } = string.Empty;
    public int Total { get; set; } = 0;
    public int InQueue { get; set; } = 0;
    public int Paid { get; set; } = 0;
    public int DroppedOut { get; set; } = 0;
}
