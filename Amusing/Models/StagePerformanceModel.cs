namespace Amusing.Models;

public class StagePerformanceModel
{
    public int StageId { get; set; }
    public string StageName { get; set; } = "";
    public int Timeslot { get; set; }
    public string GroupName { get; set; } = "";
}
