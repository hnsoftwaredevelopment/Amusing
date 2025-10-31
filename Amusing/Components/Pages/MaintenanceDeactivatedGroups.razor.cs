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

public partial class MaintenanceDeactivatedGroups : ComponentBase
{
    [Inject] protected LoggingService LoggingService { get; set; } = default!;
    [Inject] protected GroupService GroupService { get; set; } = default!;

    protected bool _initialLoadDone = false;
    protected bool IsLoading = false;
    protected GroupModel? SelectedGroup;
    protected int VisibleRowCount = 0;
    protected List<GroupModel> Groups = [];
    protected SfGrid<GroupModel>? GridRef;
    protected string FileName = "Gedeactiveerde (verwijderde) koren";
    public List<Syncfusion.Blazor.Grids.ContextMenuItemModel>? ContextMenuItems = new()
        {
            new ContextMenuItemModel { Text = "Verwijder alle niet relevante koor data", Id = "destroy" },
            new ContextMenuItemModel { Text = "Maak koor weer actief", Id = "reactivate" }
        };

    protected override async Task OnInitializedAsync()
    {
        IsLoading = true;

        Groups = await GroupService.GetInactiveGroupsAsync();

        SelectedGroup = Groups.FirstOrDefault();

        IsLoading = false;
    }

    public async Task OnInput( InputEventArgs args )
    {
        await GridRef.SearchAsync( args.Value );

        await Task.Delay( 50 );
        await UpdateVisibleRowCountAsync();
    }

    protected async Task OnGridDataBound()
    {
        if ( !_initialLoadDone && Groups?.Any() == true )
        {
            _initialLoadDone = true;
            await UpdateVisibleRowCountAsync();
            await GridRef.SelectRowAsync( 0 );
        }
    }

    protected void OnRowSelected( RowSelectEventArgs<GroupModel> args )
    {
        SelectedGroup = args.Data;

        StateHasChanged();
    }

    protected async Task UpdateVisibleRowCountAsync()
    {
        if ( GridRef is not null )
        {
            var records = await GridRef.GetCurrentViewRecordsAsync();
            await Task.Delay( 150 );
            VisibleRowCount = records?.Count ?? 0;
            StateHasChanged();
        }
    }

    protected async Task ReactivateGroup()
    {
        if ( SelectedGroup is null )
            return;
        await GroupService.ReactivateGroupAsync( SelectedGroup.GroupId );

        Groups = await GroupService.GetInactiveGroupsAsync();
        SelectedGroup = Groups
            .OrderByDescending( f => f.Name )
            .FirstOrDefault();
        await GridRef.Refresh();

        // Select the first record in the grid
        if ( Groups.Count != 0 )
        {
            SelectedGroup = Groups [ 0 ];
            await GridRef.SelectRowAsync( 0 );
        }
        else
        {
            SelectedGroup = null;
        }
    }

    public async Task OnContextMenuClick( ContextMenuClickEventArgs<GroupModel> args )
    {
        if ( args.Item.Items?.Count > 0 )
            return;

        var selected = args.RowInfo.RowData;
        var _tempName = selected.Name;
        var _tempCity = selected.City;
        var _tempId = selected.GroupId;

        if ( args.Item.Id.StartsWith( "destroy" ) )
        {
            var index = GridRef.SelectedRowIndex;
            await GroupService.DestroyGroupAsync( selected.GroupId );

            string _report = $"<_userName> heeft niet relevante data van \"{_tempName}\" uit {_tempCity} uit de database verwijderd";
            await LoggingService.WriteUserActionGroupAsync( _tempId, "Beheer", "Groepen", "success", _report );

            await Task.Delay( 150 );
            Groups = await GroupService.GetInactiveGroupsAsync();
            await Task.Delay( 50 );
            await GridRef.Refresh();
            SelectedGroup = Groups [ index ];
            await GridRef.SelectRowAsync( index );
        }
        else if ( args.Item.Id.StartsWith( "reactivate" ) )
        {
            await GroupService.ReactivateGroupAsync( selected.GroupId );

            string _report = $"<_userName> heeft \"{_tempName}\" uit {_tempCity} opnieuw geactiveerd";
            await LoggingService.WriteUserActionGroupAsync( _tempId, "Beheer", "Groepen", "success", _report );

            await ReactivateGroup();
        }
    }
}