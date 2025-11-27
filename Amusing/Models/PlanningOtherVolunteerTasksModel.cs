namespace Amusing.Models;

public class PlanningOtherVolunteerTasksModel
{
    public string TaskName { get; set; }
    public string Volunteer { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
}
