namespace Amusing.Models;

public class PlanningRegistrationsModel
{
    public uint FestivalId { get; set; }
    public uint GroupId { get; set; }
    public string GroupName { get; set; }
    public string Wish1 { get; set; }
    public string Wish2 { get; set; }
    public string Wish3 { get; set; }
    public string Wish4 { get; set; }
    public uint Singers { get; set; }
    public string Stagetype { get; set; }
    public int ForcedStageChoice { get; set; }
    public DateTime Registered { get; set; }
    public TimeOnly AvailableFrom { get; set; }
    public TimeOnly AvailableTill { get; set; }
    public uint Queue { get; set; }
    public uint InsidePerformances { get; set; }
    public uint OutsidePerformances { get; set; }
    public DateTime Confirmed { get; set; }
}
