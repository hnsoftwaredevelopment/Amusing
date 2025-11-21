namespace Amusing.Models;

public class PlanningConditionsModel
{
    public int WishTimeBetweenPerformances { get; set; }
    public int MaxTimeBetweenPerformances { get; set; }
    public int MaxLentgVolunteersShift { get; set; }
    public int PenaltyInteruptionPerformances { get; set; }
    public string TasknamesWithoutSwitchTime { get; set; } = "Vrijwilligersbalie;Garderobe";
    public string SubstitudeTaskName { get; set; } = "Reserve voor oproep";
}
