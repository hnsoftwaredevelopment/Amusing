namespace Amusing.Models;

public class PlanningFestivalsModel
{
    public uint FestivalId { get; set; }
    public string Festival { get; set; }
    public DateOnly FestivalDate { get; set; }
    public int PerformanceLength { get; set; } = 30;
    public TimeOnly StartFestivalday { get; set; }
    public TimeOnly EndFestivalday { get; set; }
    public TimeOnly StartPause { get; set; }
    public TimeOnly EndPause { get; set; }
    public TimeOnly EndExperiencedSubstitude { get; set; }
}
