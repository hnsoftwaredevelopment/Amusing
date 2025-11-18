namespace Amusing.Models;

public class DashboardSubscriptionsPivot
{
    public string DeelnemersCategorie { get; set; } = string.Empty;

    // Dynamic columns coming from the pivot
    public Dictionary<string, int> Podia { get; set; } = [];
}
