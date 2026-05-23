using System.Collections.Generic;
using System.Threading.Tasks;

using Amusing.Helpers;
using Amusing.Models;
using Amusing.Services;

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Inputs;

namespace Amusing.Components.Pages;

public partial class ListStageTypes : ComponentBase
{
    [Inject] protected LoggingService LoggingService { get; set; } = default!;
    [Inject] protected StageTypeService StageTypeService { get; set; } = default!;

    protected bool IsLoading = false;
    protected bool _initialLoadDone = false;
    protected SfGrid<StageTypeModel> GridRef;
    protected int VisibleRowCount = 0;
    protected List<StageTypeModel> StageTypes = [];
    protected string FileName = "Podium types";
    protected override async Task OnInitializedAsync()
    {
        IsLoading = true;
        StageTypes = await StageTypeService.GetStageTypesAsync();
        IsLoading = false;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            if (StageTypes?.Count > 0)
            {
                await UpdateVisibleRowCountAsync();
            }
        }
    }

    // Manage direct search functionality
    public async Task OnInput( InputEventArgs args )
    {
        await GridRef.SearchAsync( args.Value );

        await Task.Delay( 50 );
        await UpdateVisibleRowCountAsync();
    }

    protected async Task UpdateVisibleRowCountAsync()
    {
        if (GridRef == null)
            return;

        await GridRef.Refresh();
            await Task.Delay( 50 );
            var records = await GridRef.GetCurrentViewRecordsAsync();
            await Task.Delay( 150 );
            VisibleRowCount = records?.Count ?? 0;
            StateHasChanged();
    }

    protected async Task OnGridDataBound()
    {
        if ( !_initialLoadDone )
        {
            _initialLoadDone = true;
            await UpdateVisibleRowCountAsync();
        }
    }
}
