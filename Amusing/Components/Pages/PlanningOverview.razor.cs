using Amusing.Models;
using Amusing.Services;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.JSInterop;

using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Grids.Internal;

namespace Amusing.Components.Pages;

public partial class PlanningOverview
{
    protected SfGrid<PlanningPerformancesModel> PerformancesGridRef;
    protected SfGrid<PlanningVolunteerTasksModel> StageTaskGridRef;
    protected SfGrid<PlanningVolunteerTasksModel> OtherTasksGridRef;
    protected List<Edition> Editions = [];
    [Inject] public PlanningService PlanningService { get; set; } = default!;
    [Inject] protected LoggingService LoggingService { get; set; } = default!;
    [Inject] protected EditionService EditionService { get; set; } = default!;
    [Inject] public IJSRuntime JS { get; set; } = default!;

    private string _message;
    private string SelectedEditionId { get; set; }
    public string SelectedEditionText { get; set; }

    protected override async Task OnInitializedAsync()
    {
        Editions = await EditionService.GetEditionsAsync();

        if ( Editions.Count != 0 )
        {
            // Auto select the current festival edition
            SelectedEditionId = Editions
                .OrderByDescending( e => int.Parse( e.Text ) )
                .First().ID;
        }
    }

    protected async Task OnEditionChanged( string selectedId )
    {
        if ( string.IsNullOrWhiteSpace( selectedId ) )
            return;

        SelectedEditionId = selectedId;

        // Find label text
        var edition = Editions.FirstOrDefault(e => e.ID == selectedId);
        SelectedEditionText = edition?.Text ?? "";

    }

    private async Task ExportToXmlAsync()
    {
        var _editionId = 20;
        // Trigger XML export for the selected edition
        await PlanningService.ExportFullPlanningToXmlAsync(
            _editionId,
            "C:\\Temp\\Planning.xml"
        );
        _message = "Planning export completed.";
    }

    private async Task ExportToExcelAsync()
    {
    //    // LEGE placeholder – vullen we zodra de XML werkt
    await Task.CompletedTask;
    }
}