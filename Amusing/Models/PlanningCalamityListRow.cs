namespace Amusing.Models;

public class PlanningCalamityListRow
{
    public string StageName { get; set; } = string.Empty;
    public int StageNumber { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string Volunteer { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}
