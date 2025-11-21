namespace Amusing.Models;

public class PlanningPerformancesModel
{
    public uint FestivalId { get; set; }
    public uint GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public uint TimeSlotId { get; set; }
    public uint StageId { get; set; }
    public string StageName { get; set; } = string.Empty;
    public TimeOnly From { get; set; }
    public TimeOnly To { get; set; }
    public bool Pinned { get; set; }
    public string Description { get; set; } = string.Empty;
}
