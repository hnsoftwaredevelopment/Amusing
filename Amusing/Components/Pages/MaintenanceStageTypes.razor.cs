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

public partial class MaintenanceStageTypes : ComponentBase
{
    [Inject] protected LoggingService LoggingService { get; set; } = default!;
    [Inject] protected StageTypeService StageTypeService { get; set; } = default!;

    protected bool IsLoading = false;
    protected bool _initialLoadDone = false;
    protected SfGrid<StageTypeModel> GridRef;
    protected int VisibleRowCount = 0;
    protected List<StageTypeModel> StageTypes = [];
    protected string FileName = "Podium types";
    protected StageTypeModel? SelectedStageType;
    protected StageTypeModel? SelectedStageTypeOriginal;
    protected List<string> ChangedFields = new();
    protected List<string> AvailableStageTypes = [];
    protected List<string> CompatibelTypes = [];
    protected bool IsDirty = false;
    protected BitVisibility ShowDeletedMessage = BitVisibility.Collapsed;

    protected bool ElectronicsChecked
    {
        get => SelectedStageType?.Electronics == 1;
        set => SelectedStageType.Electronics = value ? 1 : 0;
    }

    protected bool MixingConsoleChecked
    {
        get => SelectedStageType?.MixingConsole == 1;
        set => SelectedStageType.MixingConsole = value ? 1 : 0;
    }

    protected bool Mp3Checked
    {
        get => SelectedStageType?.Mp3 == 1;
        set => SelectedStageType.Mp3 = value ? 1 : 0;
    }

    protected bool ActiveChecked
    {
        get => SelectedStageType?.Active == 1;
        set => SelectedStageType.Active = value ? 1 : 0;
    }

    public string StageTypeName
    {
        get => SelectedStageType?.Type ?? string.Empty;
        set
        {
            if (SelectedStageType != null)
            {
                SelectedStageType.Type = value;
            }
        }
    }


    public decimal StageTypePrice
    {
        get => SelectedStageType?.Price ?? 0m;
        set { if (SelectedStageType != null) SelectedStageType.Price = value; }
    }

    public int StageTypePiano
    {
        get => SelectedStageType?.Piano ?? 0;
        set { if (SelectedStageType != null) SelectedStageType.Piano = value; }
    }

    public int StageTypeLectern
    {
        get => SelectedStageType?.Lectern ?? 0;
        set { if (SelectedStageType != null) SelectedStageType.Lectern = value; }
    }

    public int StageTypeMicrophones
    {
        get => SelectedStageType?.Microphones ?? 0;
        set { if (SelectedStageType != null) SelectedStageType.Microphones = value; }
    }

    public int StageTypeDrums
    {
        get => SelectedStageType?.Drums ?? 0;
        set { if (SelectedStageType != null) SelectedStageType.Drums = value; }
    }

    public int StageTypeGuitarAmplifiers
    {
        get => SelectedStageType?.GuitarAmplifiers ?? 0;
        set { if (SelectedStageType != null) SelectedStageType.GuitarAmplifiers = value; }
    }

    public int StageTypeBassAmplifiers
    {
        get => SelectedStageType?.BassAmplifiers ?? 0;
        set { if (SelectedStageType != null) SelectedStageType.BassAmplifiers = value; }
    }

    public int StageTypeChoirAmplifiers
    {
        get => SelectedStageType?.ChoirAmplifiers ?? 0;
        set { if (SelectedStageType != null) SelectedStageType.ChoirAmplifiers = value; }
    }

    public int StageTypeMonitors
    {
        get => SelectedStageType?.Monitors ?? 0;
        set { if (SelectedStageType != null) SelectedStageType.Monitors = value; }
    }

    public int StageTypeSpeakers
    {
        get => SelectedStageType?.Speakers ?? 0;
        set { if (SelectedStageType != null) SelectedStageType.Speakers = value; }
    }

    public int StageTypeActive
    {
        get => SelectedStageType?.Active ?? 0;
        set { if (SelectedStageType != null) SelectedStageType.Active = value; }
    }

    protected override async Task OnInitializedAsync()
    {
        IsLoading = true;

        var stageTypeModels = await StageTypeService.GetActiveStageTypesListAsync();
        AvailableStageTypes = stageTypeModels
                                .Select( s => s.Type )
                                .Where( t => !string.IsNullOrWhiteSpace( t ) )
                                .Distinct()
                                .ToList();

        StageTypes = await StageTypeService.GetAllStageTypesAsync();
        SelectedStageType = StageTypes.FirstOrDefault( x => x.Type == "A" );
        IsLoading = false;
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

    protected async Task OnGridDataBound()
    {
        if ( !_initialLoadDone )
        {
            _initialLoadDone = true;
            await UpdateVisibleRowCountAsync();
        }
    }

    protected void OnRowSelected( RowSelectEventArgs<StageTypeModel> args )
    {
        SelectedStageType = args.Data;

        // Clone the selected row, to compare changed values with the original values
        SelectedStageTypeOriginal = new StageTypeModel
        {
            Type = args.Data.Type,
            Price = args.Data.Price,
            Piano = args.Data.Piano,
            Lectern = args.Data.Lectern,
            Electronics = args.Data.Electronics,
            Drums = args.Data.Drums,
            GuitarAmplifiers = args.Data.GuitarAmplifiers,
            BassAmplifiers = args.Data.BassAmplifiers,
            ChoirAmplifiers = args.Data.ChoirAmplifiers,
            Microphones = args.Data.Microphones,
            Monitors = args.Data.Monitors,
            Speakers = args.Data.Speakers,
            MixingConsole = args.Data.MixingConsole,
            Mp3 = args.Data.Mp3,
            Compatible = args.Data.Compatible,
            Active = args.Data.Active
        };

        CompatibelTypes = SelectedStageType.Compatible?
        .Split( ',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries )
        .ToList() ?? new List<string>();

        ShowDeletedMessage = BitVisibility.Collapsed;
    }

    protected async Task SaveStageType()
    {
        if ( SelectedStageType is null )
            return;

        // Make sure the selected compatible stage types are sorted before save
        SelectedStageType.Compatible = string.Join( ",", CompatibelTypes
        .Where( t => !string.IsNullOrWhiteSpace( t ) )
        .OrderBy( t => t ) );

        await UpdateIsDirty();

        StageTypes = await StageTypeService.GetAllStageTypesAsync();
        await GridRef.Refresh();
        await GridRef.SelectRowAsync( StageTypes.IndexOf( SelectedStageType ) );

    }

    // Check if content of a Field is changed after selecting a row
    protected async Task UpdateIsDirty()
    {
        ChangedFields.Clear();

        var compatibelSorted = string.Join(",", CompatibelTypes.OrderBy(x => x));
        var originalCompatibelSorted = string.Join(",", (SelectedStageTypeOriginal.Compatible ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).OrderBy(x => x));

        bool vuilelucht = SelectedStageType != SelectedStageTypeOriginal;

        IsDirty = SelectedStageType.Type != SelectedStageTypeOriginal.Type ||
                  SelectedStageType.Price != SelectedStageTypeOriginal.Price ||
                  SelectedStageType.Piano != SelectedStageTypeOriginal.Piano ||
                  SelectedStageType.Lectern != SelectedStageTypeOriginal.Lectern ||
                  SelectedStageType.Electronics != SelectedStageTypeOriginal.Electronics ||
                  SelectedStageType.Drums != SelectedStageTypeOriginal.Drums ||
                  SelectedStageType.GuitarAmplifiers != SelectedStageTypeOriginal.GuitarAmplifiers ||
                  SelectedStageType.BassAmplifiers != SelectedStageTypeOriginal.BassAmplifiers ||
                  SelectedStageType.ChoirAmplifiers != SelectedStageTypeOriginal.ChoirAmplifiers ||
                  SelectedStageType.Microphones != SelectedStageTypeOriginal.Microphones ||
                  SelectedStageType.Monitors != SelectedStageTypeOriginal.Monitors ||
                  SelectedStageType.Speakers != SelectedStageTypeOriginal.Speakers ||
                  SelectedStageType.MixingConsole != SelectedStageTypeOriginal.MixingConsole ||
                  SelectedStageType.Mp3 != SelectedStageTypeOriginal.Mp3 ||
                  compatibelSorted != originalCompatibelSorted ||
                  SelectedStageType.Active != SelectedStageTypeOriginal.Active;

        // When dirty, save a new version of the StageType
        if ( IsDirty )
        {
            var newVersion = await StageTypeService.GetNewStageTypeVersionByTypeAsync(SelectedStageType.Type);
            SelectedStageType.Version = newVersion;
            await StageTypeService.InsertStageTypeAsync( SelectedStageType );

            // Check the differences between the original and cjhanged version
            var differences = ObjectDiffHelper.GetDifferences(SelectedStageTypeOriginal, SelectedStageType);

            if ( differences.Count > 0 )
            {
                string stageName = SelectedStageType.Type;

                foreach ( var diff in differences )
                {
                    string logMessage =
                $"<_userName> heeft {diff.PropertyName} van podiumtype \"{SelectedStageType.Type}\" versie: {SelectedStageType.Version} gewijzigd van '{diff.OldValue}' in '{diff.NewValue}'.";

                    await LoggingService.WriteUserActionStageTypeAsync( SelectedStageType.Type, "Beheer", "Podiumtype", "updated", logMessage );
                }
            }
        }
    }

    protected async Task AddNewStageType()
    {
        var newType = await StageTypeService.GetNextAvailableStageTypeAsync();

        SelectedStageType = new StageTypeModel
        {
            Type = newType,
            Price = 0,
            Piano = 1,
            Lectern = 1,
            Electronics = 1,
            Drums = 0,
            GuitarAmplifiers = 0,
            BassAmplifiers = 0,
            ChoirAmplifiers = 0,
            Microphones = 0,
            Monitors = 0,
            Speakers = 0,
            MixingConsole = 0,
            Mp3 = 0,
            Beschrijving = " ",
            Description = " ",
            Compatible = newType,
            Version = 1,
            Active = 1
        };

        // Save the new record in the database
        await StageTypeService.InsertStageTypeAsync( SelectedStageType );

        var logMessage = $"<_userName> heeft nieuw podiumtype \"{SelectedStageType.Type}\" toegevoegd.";
        await LoggingService.WriteUserActionStageTypeAsync( SelectedStageType.Type, "Beheer", "Podiumtype", "added", logMessage );


        // refresh the table
        var stageTypeModels = await StageTypeService.GetActiveStageTypesListAsync();
        AvailableStageTypes = stageTypeModels
                                .Select( s => s.Type )
                                .Where( t => !string.IsNullOrWhiteSpace( t ) )
                                .Distinct()
                                .ToList();

        StageTypes = await StageTypeService.GetAllStageTypesAsync();

        await GridRef.Refresh();

        // select the newly created StageType in the list
        SelectedStageType = StageTypes.FirstOrDefault( x => x.Type == newType );


        await GridRef.SelectRowAsync( StageTypes.IndexOf( SelectedStageType ) );
    }

    protected async Task DeleteStageType( string type, int version )
    {
        await StageTypeService.DeleteStageTypeAsync( type, version );

        var logMessage = $"<_userName> heeft podiumtype \"{SelectedStageType.Type}\" versie: {SelectedStageType.Version} verwijderd.";
        await LoggingService.WriteUserActionStageTypeAsync( SelectedStageType.Type, "Beheer", "Podiumtype", "deleted", logMessage );

    }

    protected async Task AddStageTypeClicked()
    {
        await AddNewStageType();
    }

    protected async Task DeleteStageTypeClicked()
    {
        string type = SelectedStageType.Type.ToString();
        int version = ( int ) SelectedStageType.Version;

        if ( type != "" && version != 0 )
        {
            ShowDeletedMessage = BitVisibility.Visible;
            await DeleteStageType( type, version );

            StageTypes = await StageTypeService.GetAllStageTypesAsync();
            await GridRef.Refresh();
            await GridRef.SelectRowAsync( StageTypes.IndexOf( SelectedStageType ) );
        }
    }

}