namespace Amusing.Models;

public class PlanningStagesModel
{
    public uint PodiumId { get; set; }
    public string Name { get; set; }
    public string PerformanceLocation { get; set; }
    public string Type { get; set; }
    public uint Quality { get; set; }
    public uint MaxSingers { get; set; }
    public string Volunteers { get; set; }
    public TimeOnly Opening { get; set; }
    public TimeOnly Closing { get; set; }
    public TimeOnly VolunteersFrom { get; set; }
    public TimeOnly VolunteersTill { get; set; }
    public uint MapNumber { get; set; }
}
