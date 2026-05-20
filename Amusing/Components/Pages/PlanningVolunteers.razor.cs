using Amusing.Models;
using Amusing.Services;

using Microsoft.AspNetCore.Components;

namespace Amusing.Components.Pages;

public partial class PlanningVolunteers
{
    [Inject] public PlanningService PlanningService { get; set; } = default!;
    [Inject] protected EditionService EditionService { get; set; } = default!;

    protected List<Edition> Editions { get; set; } = [];
    protected string? SelectedEditionId { get; set; }
    protected string SelectedEditionText { get; set; } = string.Empty;

    protected List<PlanningVolunteerOverviewRow> ByVolunteer { get; set; } = [];
    protected List<PlanningVolunteerOverviewRow> ByTask { get; set; } = [];
    protected List<PlanningVolunteerOverviewRow> ByStage { get; set; } = [];

    protected override async Task OnInitializedAsync()
    {
        Editions = await EditionService.GetEditionsAsync();

        if (Editions.Count == 0)
            return;

        SelectedEditionId = Editions
            .OrderByDescending(e => int.Parse(e.Text))
            .First().ID;

        SetSelectedEditionText();
        await LoadPlanningAsync();
    }

    protected async Task OnEditionChanged(string selectedId)
    {
        if (string.IsNullOrWhiteSpace(selectedId))
            return;

        SelectedEditionId = selectedId;
        SetSelectedEditionText();
        await LoadPlanningAsync();
    }

    private async Task LoadPlanningAsync()
    {
        if (!int.TryParse(SelectedEditionId, out var festivalId))
            return;

        ByVolunteer = await PlanningService.GetPlanningVolunteerOverviewByVolunteerAsync(festivalId);
        ByTask = await PlanningService.GetPlanningVolunteerOverviewByTaskAsync(festivalId);
        ByStage = await PlanningService.GetPlanningVolunteerOverviewByStageAsync(festivalId);
    }

    private void SetSelectedEditionText()
    {
        SelectedEditionText = Editions.FirstOrDefault(e => e.ID == SelectedEditionId)?.Text ?? string.Empty;
    }

    private PlanningVolunteerOverviewRow? GetPreviousVolunteer(PlanningVolunteerOverviewRow current)
    {
        var index = ByVolunteer.IndexOf(current);
        return index <= 0 ? null : ByVolunteer[index - 1];
    }

    private PlanningVolunteerOverviewRow? GetPreviousTask(PlanningVolunteerOverviewRow current)
    {
        var index = ByTask.IndexOf(current);
        return index <= 0 ? null : ByTask[index - 1];
    }

    private PlanningVolunteerOverviewRow? GetPreviousStage(PlanningVolunteerOverviewRow current)
    {
        var index = ByStage.IndexOf(current);
        return index <= 0 ? null : ByStage[index - 1];
    }
}
