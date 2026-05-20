namespace Amusing.Models;

public class PlanningVolunteerOverviewRow
{
    public int PersonId { get; set; }
    public string Volunteer { get; set; } = string.Empty;
    public string Contact { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Fixed { get; set; } = string.Empty;
}
