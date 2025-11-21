namespace Amusing.Models;

public class PlanningVolunteerTasksModel
{
    public uint TaakId { get; set; }
    public string ShortName { get; set; }
    public string Name { get; set; }
    public uint MinimumTime { get; set; }
    public uint MaximumTime { get; set; }
    public TimeOnly Timeslot1From { get; set; }
    public TimeOnly Timeslot1Till { get; set; }
    public int Timeslot1Volunteers { get; set; }
    public TimeOnly Timeslot2From { get; set; }
    public TimeOnly Timeslot2Till { get; set; }
    public int Timeslot2Volunteers { get; set; }

}
