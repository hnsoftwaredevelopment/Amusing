using Amusing.Helpers;
using Amusing.Models;
using Amusing.Services;

using Microsoft.AspNetCore.Components;

using Syncfusion.Blazor.Data;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Inputs;
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
    protected List<StageTypeModel> AvailableStageTypes = [];
    protected string? SelectedStageType;
    protected int VisibleRowCount = 0;
    protected List<StageModel> Stages = [];
    protected List<StageModel> FilteredStages = [];

    protected SfTooltip? TooltipObj;
    protected bool isOpen = false;

    private bool _showOnlyActiveStages;
    protected bool ShowOnlyActiveStages => _showOnlyActiveStages;

    protected string SelectedStageTypeText => AvailableStageTypes.FirstOrDefault(e => e.Type == SelectedStageType)?.Type ?? "Onbekende podium type";

    protected bool ActiveChecked
    {
        get => SelectedStage?.Aktief == 1;
        set => SelectedStage.Aktief = value ? 1 : 0;
    }

    // ========================
    // String wrappers
    // ========================
    public string StageNaam
    {
        get => SelectedStage?.Naam ?? string.Empty;
        set { if (SelectedStage != null) SelectedStage.Naam = value; }
    }

    public string StageSoort
    {
        get => SelectedStage?.Soort ?? string.Empty;
        set { if (SelectedStage != null) SelectedStage.Soort = value; }
    }

    public string StageType
    {
        get => SelectedStage?.Type ?? string.Empty;
        set { if (SelectedStage != null) SelectedStage.Type = value; }
    }

    // ========================
    // Integer wrappers
    // ========================
    public int StageKaartNummer
    {
        get => SelectedStage?.KaartNummer ?? 0;
        set { if (SelectedStage != null) SelectedStage.KaartNummer = value; }
    }

    public int StageKwaliteit
    {
        get => SelectedStage?.Kwaliteit ?? 0;
        set { if (SelectedStage != null) SelectedStage.Kwaliteit = value; }
    }

    public int StageMaxZangers
    {
        get => SelectedStage?.MaxZangers ?? 0;
        set { if (SelectedStage != null) SelectedStage.MaxZangers = value; }
    }

    public int StageVrijwilligers
    {
        get => SelectedStage?.Vrijwilligers ?? 0;
        set { if (SelectedStage != null) SelectedStage.Vrijwilligers = value; }
    }

    // ========================
    // TimeOnly wrappers
    // ========================
    public TimeOnly StageStart
    {
        get => SelectedStage?.Start ?? TimeOnly.MinValue;
        set { if (SelectedStage != null) SelectedStage.Start = value; }
    }

    public TimeOnly StageEind
    {
        get => SelectedStage?.Eind ?? TimeOnly.MinValue;
        set { if (SelectedStage != null) SelectedStage.Eind = value; }
    }

    public TimeOnly StageVan
    {
        get => SelectedStage?.Van ?? TimeOnly.MinValue;
        set { if (SelectedStage != null) SelectedStage.Van = value; }
    }

    public TimeOnly StageTot
    {
        get => SelectedStage?.Tot ?? TimeOnly.MinValue;
        set { if (SelectedStage != null) SelectedStage.Tot = value; }
    }

    protected override async Task OnInitializedAsync()
    {
        IsLoading = true;

        Stages = await StageService.GetAllStagesAsync();
        ApplyStageFilter();
        AvailableStageTypes = await StageTypeService.GetAllStageTypesListAsync();
        SelectedStage = FilteredStages
            .OrderByDescending(f => f.Naam)
            .FirstOrDefault();
        IsLoading = false;
    }

    protected async Task OnGridDataBound()
    {
        if (!_initialLoadDone)
        {
            _initialLoadDone = true;
            await UpdateVisibleRowCountAsync();
        }
    }

    protected async Task UpdateVisibleRowCountAsync()
    {
        if (GridRef is not null)
        {
            var records = await GridRef.GetCurrentViewRecordsAsync();
            VisibleRowCount = records?.Count ?? 0;
            StateHasChanged();
        }
    }

    private void ApplyStageFilter()
    {
        FilteredStages = ShowOnlyActiveStages
            ? Stages.Where(stage => stage.KaartNummer > 0).ToList()
            : Stages.ToList();
    }

    private async Task ApplyStageFilterAsync()
    {
        ApplyStageFilter();

        if (GridRef is not null)
        {
            await GridRef.Refresh();
            await UpdateVisibleRowCountAsync();
        }

        if (SelectedStage is not null && !FilteredStages.Any(stage => stage.PodiumId == SelectedStage.PodiumId))
        {
            SelectedStage = FilteredStages.FirstOrDefault();
        }

        StateHasChanged();
    }

    protected async Task OnShowOnlyActiveStagesChanged(ChangeEventArgs args)
    {
        _showOnlyActiveStages = args.Value is bool value && value;
        await ApplyStageFilterAsync();
    }

    public async Task OnInput(InputEventArgs args)
    {
        await GridRef.SearchAsync(args.Value);

        await Task.Delay(50);
        await UpdateVisibleRowCountAsync();
    }

    protected void OnRowSelected(RowSelectEventArgs<StageModel> args)
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

    public async Task OnSearchInput(InputEventArgs args)
    {
        if (GridRef is null)
            return;

        var searchText = args?.Value?.ToString() ?? string.Empty;

        await GridRef.SearchAsync(searchText);

        var records = await GridRef.GetCurrentViewRecordsAsync();
        VisibleRowCount = records?.Count ?? 0;

        StateHasChanged();
    }

    protected async Task SaveStage()
    {
        if (SelectedStage is null)
            return;

        // Check the differences between the original and cjhanged version
        var differences = ObjectDiffHelper.GetDifferences(SelectedStageOriginal, SelectedStage);

        if (differences.Count > 0)
        {
            string stageName = SelectedStage.Naam;

            foreach (var diff in differences)
            {
                string logMessage =
                $"<_userName> heeft {diff.PropertyName} van podium \"{stageName}\" gewijzigd van '{diff.OldValue}' in '{diff.NewValue}'.";

                await LoggingService.WriteUserActionStageAsync(SelectedStage.PodiumId, "Beheer", "Podia", "updated", logMessage);
            }
        }

        // Save te current Index of the record in the grid
        var savedId = SelectedStage.PodiumId;

        await StageService.ModifyStageAsync(SelectedStage);

        // Refresh the list
        Stages = await StageService.GetAllStagesAsync();
        ApplyStageFilter();
        await GridRef.Refresh();

        // Search the modified record
        var index = FilteredStages.FindIndex(s => s.PodiumId == savedId);
        if (index >= 0)
        {
            SelectedStage = FilteredStages[index];
            await GridRef.SelectRowAsync(index);
        }
        else
        {
            SelectedStage = FilteredStages.FirstOrDefault();
        }

        await UpdateVisibleRowCountAsync();
    }

    protected async Task AddNewStage()
    {
        if (SelectedStage is null)
            return;

        // Insert new record for this year in the table, and get the Stage-Id
        var stageId = await StageService.InsertNewStageAsync();

        var logMessage = $"<_userName> heeft podium met Id:{stageId} toegevoegd.";
        await LoggingService.WriteUserActionStageAsync(stageId, "Beheer", "Podia", "added", logMessage);

        // Refresh the list
        Stages = await StageService.GetAllStagesAsync();
        ApplyStageFilter();
        await GridRef.Refresh();

        // Search the new record
        var index = FilteredStages.FindIndex(s => s.PodiumId == stageId);
        if (index >= 0)
        {
            SelectedStage = FilteredStages[index];
            await GridRef.SelectRowAsync(index);
        }
        else
        {
            SelectedStage = FilteredStages.FirstOrDefault();
        }

        await UpdateVisibleRowCountAsync();
    }

    protected async Task DeleteStage()
    {
        if (SelectedStage is null)
            return;

        await StageService.DeleteStageAsync(SelectedStage.PodiumId);

        var logMessage = $"<_userName> heeft podium \"{SelectedStage.Naam}\" verwijderd.";
        await LoggingService.WriteUserActionStageAsync(SelectedStage.PodiumId, "Beheer", "Podia", "deleted", logMessage);

        // refresh the table
        var stageModels = await StageService.GetAllStagesAsync();

        Stages = await StageService.GetAllStagesAsync();
        ApplyStageFilter();
        SelectedStage = FilteredStages
            .OrderByDescending(f => f.Naam)
            .FirstOrDefault();
        await GridRef.Refresh();

        // Select the first record in the grid
        if (FilteredStages.Any())
        {
            SelectedStage = FilteredStages[0];
            await GridRef.SelectRowAsync(0);
        }
        else
        {
            SelectedStage = null;
        }

        await UpdateVisibleRowCountAsync();
    }

    protected void OnOpen(Syncfusion.Blazor.DropDowns.PopupEventArgs args)
    {
        isOpen = true;
    }

    protected async Task OnClose(Syncfusion.Blazor.DropDowns.PopupEventArgs args)
    {
        if (TooltipObj != null)
        {
            await TooltipObj.CloseAsync();
        }
        isOpen = false;
    }

    // List for Kind of Stage (In or Out)
    public string ComboBoxValue { get; set; } = "Buiten";
    public string[] dataStageKind = { "Binnen", "Buiten" };
}
