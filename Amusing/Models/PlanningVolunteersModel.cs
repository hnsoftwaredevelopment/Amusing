namespace Amusing.Models;

public class PlanningVolunteersModel
{
    public uint VolunteerId { get; set; }
    public DateTime Date { get; set; }
    public uint FestivalId { get; set; }
    public uint PersonId { get; set; }
    public string PersonName { get; set; }
    public TimeOnly AvailableFrom { get; set; }
    public TimeOnly AvailableTill { get; set; }
    public uint ChainedHours { get; set; }
    public string Lunch { get; set; }
    public string Vegetarian { get; set; }
    public string Meeting { get; set; }
    public string Experience { get; set; }
    public string StageDuty { get; set; }
    public string Tasks { get; set; }
    public uint TogetherWithId { get; set; }
    public string TogetherWithName { get; set; }
    public uint PreferedStage { get; set; }
    public uint DisapprovedStage { get; set; }
    public uint PreferedGroup { get; set; }
    public uint DisapprovedGroup { get; set; }
    public string PreferedTask { get; set; }
    public string DisapprovedTask { get; set; }
    public string Notes { get; set; }
}
