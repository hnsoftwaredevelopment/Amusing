using System.ComponentModel.DataAnnotations;

using Amusing.Helpers;
using Amusing.Models;
using Amusing.Services;

using Bit.BlazorUI;
using Bit.BlazorUI.Extras;

using Microsoft.AspNetCore.Components;

using Syncfusion.Blazor.Buttons;
using Syncfusion.Blazor.Calendars;
using Syncfusion.Blazor.Data;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Inputs;
using Syncfusion.Blazor.Navigations;
using Syncfusion.Blazor.Popups;

namespace Amusing.Components.Pages;

public partial class MaintenanceStages : ComponentBase
{
    [Inject] protected LoggingService LoggingService { get; set; } = default!;
    [Inject] protected StageService StageService { get; set; } = default!;
    [Inject] protected StageTypeService StageTypeService { get; set; } = default!;

    protected bool IsLoading = false;
    protected bool _initialLoadDone = false;
    protected SfGrid<StageModel> GridRef;
    protected string FileName = "Podia";
    protected StageModel? SelectedStage;
    protected StageModel? SelectedStageOriginal;
    protected List<StageTypeModel> AvailableStageTypes = new();
    protected string? SelectedStageType;
    protected int VisibleRowCount = 0;
    protected List<StageModel> Stages = [];

    protected SfTooltip? TooltipObj;
    protected bool isOpen = false;

    protected string SelectedStageTypeText => AvailableStageTypes.FirstOrDefault( e => e.Type == SelectedStageType )?.Type ?? "Onbekende podium type";

    protected bool ActiveChecked
    {
        get => SelectedStage?.Aktief == 1;
        set => SelectedStage.Aktief = value ? 1 : 0;
    }

    protected override async Task OnInitializedAsync()
    {
        IsLoading = true;

        Stages = await StageService.GetAllStagesAsync();
        AvailableStageTypes = await StageTypeService.GetAllStageTypesListAsync();
        SelectedStage = Stages
            .OrderByDescending( f => f.Naam )
            .FirstOrDefault();
        IsLoading = false;
    }

    protected async Task OnGridDataBound()
    {
        if ( !_initialLoadDone )
        {
            _initialLoadDone = true;
            await UpdateVisibleRowCountAsync();
        }
    }

    protected async Task UpdateVisibleRowCountAsync()
    {
        if ( GridRef is not null )
        {
            await GridRef.Refresh();
            await Task.Delay( 50 );
            var records = await GridRef.GetCurrentViewRecordsAsync();
            await Task.Delay( 150 );
            VisibleRowCount = records?.Count ?? 0;
            StateHasChanged();
        }
    }

    public async Task OnInput( InputEventArgs args )
    {
        await GridRef.SearchAsync( args.Value );

        await Task.Delay( 50 );
        await UpdateVisibleRowCountAsync();
    }

    protected void OnRowSelected( RowSelectEventArgs<StageModel> args )
    {
        SelectedStage = args.Data;


        // Clone the selected row, to compare changed values with the original values
        SelectedStageOriginal = new StageModel
        {
            PodiumId = args.Data.PodiumId,
            Naam = args.Data.Naam,
            Nfve = args.Data.Nfve,
            Soort = args.Data.Soort,
            Type = args.Data.Type,
            Kwaliteit = args.Data.Kwaliteit,
            MaxZangers = args.Data.MaxZangers,
            Vrijwilligers = args.Data.Vrijwilligers,
            Start = args.Data.Start,
            Eind = args.Data.Eind,
            Van = args.Data.Van,
            Tot = args.Data.Tot,
            KaartNummer = args.Data.KaartNummer,
            Aktief = args.Data.Aktief
        };

        StateHasChanged();
    }

    protected async Task SaveStage()
    {
        if ( SelectedStage is null )
            return;

        // Check the differences between the original and cjhanged version
        var differences = ObjectDiffHelper.GetDifferences(SelectedStageOriginal, SelectedStage);

        if ( differences.Count > 0 )
        {
            string stageName = SelectedStage.Naam;

            foreach ( var diff in differences )
            {
                string logMessage =
                $"<_userName> heeft {diff.PropertyName} van podium \"{stageName}\" gewijzigd van '{diff.OldValue}' in '{diff.NewValue}'.";

                await LoggingService.WriteUserActionStageAsync(SelectedStage.PodiumId, "Beheer", "Podia", "updated", logMessage );
            }
        }

        // Save te current Index of the record in the grid
        var savedId = SelectedStage.PodiumId;

        await StageService.ModifyStageAsync( SelectedStage );

        // Refresh the list
        Stages = await StageService.GetAllStagesAsync();
        await GridRef.Refresh();

        // Search the modified record
        var index = Stages.FindIndex(s => s.PodiumId == savedId);
        if ( index >= 0 )
        {
            SelectedStage = Stages [ index ];
            await GridRef.SelectRowAsync( index );
        }
    }

    protected async Task AddNewStage()
    {
        if ( SelectedStage is null )
            return;

        // Insert new record for this year in the table, and get the Stage-Id
        var stageId = await StageService.InsertNewStageAsync();

        var logMessage = $"<_userName> heeft podium met Id:{stageId} toegevoegd.";
        await LoggingService.WriteUserActionStageAsync( stageId, "Beheer", "Podia", "added", logMessage );

        // Refresh the list
        Stages = await StageService.GetAllStagesAsync();
        await GridRef.Refresh();

        // Search the new record
        var index = Stages.FindIndex(s => s.PodiumId == stageId);
        if ( index >= 0 )
        {
            SelectedStage = Stages [ index ];
            await GridRef.SelectRowAsync( index );
        }
    }

    protected async Task DeleteStage()
    {
        if ( SelectedStage is null )
            return;

        await StageService.DeleteStageAsync( SelectedStage.PodiumId );

        var logMessage = $"<_userName> heeft podium \"{SelectedStage.Naam}\" verwijderd.";
        await LoggingService.WriteUserActionStageAsync( SelectedStage.PodiumId, "Beheer", "Podia", "deleted", logMessage );

        // refresh the table
        var stageModels = await StageService.GetAllStagesAsync();

        Stages = await StageService.GetAllStagesAsync();
        SelectedStage = Stages
            .OrderByDescending( f => f.Naam )
            .FirstOrDefault();
        await GridRef.Refresh();

        // Select the first record in the grid
        if ( Stages.Any() )
        {
            SelectedStage = Stages [ 0 ];
            await GridRef.SelectRowAsync( 0 );
        }
        else
        {
            SelectedStage = null;
        }
    }

    protected void OnOpen( Syncfusion.Blazor.DropDowns.PopupEventArgs args )
    {
        isOpen = true;
    }

    protected async Task OnClose( Syncfusion.Blazor.DropDowns.PopupEventArgs args )
    {
        if ( TooltipObj != null )
        {
            await TooltipObj.CloseAsync();
        }
        isOpen = false;
    }

    // List for Kind of Stage (In or Out)
    public string ComboBoxValue { get; set; } = "Buiten";
    public string[] dataStageKind = { "Binnen", "Buiten" };
}
 