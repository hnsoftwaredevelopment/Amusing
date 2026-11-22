using Amusing.Models;
using Amusing.Services;

using Microsoft.AspNetCore.Components;

using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Inputs;

namespace Amusing.Components.Pages;

public partial class Logbook : ComponentBase
{
    [Inject] private LoggingService LoggingService { get; set; } = default!;

    private LogModel? _selectedReport;
    private bool _isLoading;
    private SfGrid<LogModel>? _grid;
    private List<LogModel> _logs = [];
    private bool _groupsCollapsedAfterBind;
    private readonly string[] _groupedColumns = [ "LogArea", "LogAction" ];
    private readonly string[] _searchFields = [ "LogDate", "LogUsername", "LogArea", "LogAction", "LogReport", "LogStatus" ];

    protected override async Task OnInitializedAsync()
    {
        _isLoading = true;
        _logs = await LoggingService.GetUsersLogAsync();
        _isLoading = false;
    }

    private void CloseReport() { _selectedReport = null; StateHasChanged(); }

    private void ShowReport(LogModel entry) { _selectedReport = entry; StateHasChanged(); }

    private static string GetStatusLabel(LogModel entry)
    {
        var match = System.Text.RegularExpressions.Regex.Match(entry.LogReport ?? "", @"Verzonden: (\d+), mislukt: (\d+), totaal: (\d+)");
        if (match.Success && int.TryParse(match.Groups[1].Value, out var sent) && int.TryParse(match.Groups[2].Value, out var failed))
            return failed > 0 ? (sent > 0 ? "Gedeeltelijk verzonden" : "Niet verzonden") : "Verzonden";
        return entry.LogStatus switch { "success" or "successful" => "Geslaagd", "unsuccessful" => "Mislukt", _ => entry.LogStatus };
    }

    private async Task OnInput( InputEventArgs args )
    {
        if ( _grid is null )
            return;

        await _grid.SearchAsync( args.Value );
    }

    private async Task ExpandAllGroupsAsync()
    {
        if ( _grid is null )
            return;

        await _grid.ExpandAllGroupAsync();
    }

    private async Task CollapseAllGroupsAsync()
    {
        if ( _grid is null )
            return;

        await _grid.CollapseAllGroupAsync();
    }

    private async Task OnGridDataBound()
    {
        if ( _grid is null || _groupsCollapsedAfterBind )
            return;

        _groupsCollapsedAfterBind = true;
        await _grid.CollapseAllGroupAsync();
    }
}
