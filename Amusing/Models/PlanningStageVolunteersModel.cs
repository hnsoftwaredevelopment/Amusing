namespace Amusing.Models;

public class PlanningStageVolunteersModel
{
    public int StageNumber { get; set; }
    public int StageId { get; set; }
    public string StageName { get; set; }
    public string Volunteer { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
}
