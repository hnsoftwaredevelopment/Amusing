using Amusing.Models;
using Amusing.Services;

using Microsoft.AspNetCore.Components;

using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Inputs;

namespace Amusing.Components.Pages;

public partial class Logbook : ComponentBase
{
    [Inject] private LoggingService LoggingService { get; set; } = default!;

    private bool _isLoading;
    private SfGrid<LogModel>? _grid;
    private List<LogModel> _logs = [];
    private readonly string[] _groupedColumns = [ "LogArea", "LogAction" ];
    private readonly string[] _searchFields = [ "LogDate", "LogUsername", "LogArea", "LogAction", "LogReport", "LogStatus" ];

    protected override async Task OnInitializedAsync()
    {
        _isLoading = true;
        _logs = await LoggingService.GetUsersLogAsync();
        _isLoading = false;
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
}
