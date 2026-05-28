using Amusing.Mobile.Shared.Models;

namespace Amusing.Mobile.Models;

public sealed class MobilePlanningState
{
    public MobileFestivalPlanningDto? Planning { get; set; }
    public bool IsLoading { get; set; } = true;
    public bool IsUsingCache { get; set; }
    public string? ErrorMessage { get; set; }
    public string SearchText { get; set; } = string.Empty;
    public bool ShowSelectionOnly { get; set; }
    public IReadOnlySet<uint> SelectedChoirIds { get; set; } = new HashSet<uint>();
}
