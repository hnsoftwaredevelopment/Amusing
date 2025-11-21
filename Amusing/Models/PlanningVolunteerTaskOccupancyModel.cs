namespace Amusing.Models;

public class PlanningVolunteerTaskOccupancyModel
{
    public uint TaskId { get; set; }
    public string TaskName { get; set; }
    public uint PersonId { get; set; }
    public string PersonName { get; set; }
    public int StageId { get; set; }
    public string StageName { get; set; }
    public TimeOnly From { get; set; }
    public TimeOnly Till { get; set; }
    public string Pinned { get; set; }
}
