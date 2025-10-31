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

namespace Amusing.Components.Pages;

public partial class MaintenanceFestivals : ComponentBase
{
    [Inject] protected LoggingService LoggingService { get; set; } = default!;
    [Inject] protected FestivalService FestivalService { get; set; } = default!;

    protected bool IsLoading = false;
    protected bool _initialLoadDone = false;
    protected SfGrid<FestivalModel> GridRef;
    protected string FileName = "Amusing edities";
    protected FestivalModel? SelectedFestival;
    protected FestivalModel? SelectedFestivalOriginal;
    protected List<string> ChangedFields = new();
    protected List<string> AvailableStageTypes = [];
    protected List<string> CompatibelTypes = [];
    protected bool FestivalIsDirty = false;
    protected bool ConditionsIsDirty = false;
    protected int VisibleRowCount = 0;
    protected List<FestivalModel> Festivals = [];

    protected bool WaitlistChecked
    {
        get => SelectedFestival?.Wachtlijst == 1;
        set => SelectedFestival.Wachtlijst = value ? 1 : 0;
    }

    protected bool PublicateChecked
    {
        get => SelectedFestival?.PubliceerPlanning == 1;
        set => SelectedFestival.PubliceerPlanning = value ? 1 : 0;
    }

    protected bool ActiveChecked
    {
        get => SelectedFestival?.Aktief == 1;
        set => SelectedFestival.Aktief = value ? 1 : 0;
    }

    protected override async Task OnInitializedAsync()
    {
        IsLoading = true;

        var festivalModels = await FestivalService.GetFestivalDataAsync();

        Festivals = await FestivalService.GetFestivalDataAsync();
        SelectedFestival = Festivals
            .OrderByDescending( f => int.Parse( f.Festival ) )
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

    protected void OnRowSelected( RowSelectEventArgs<FestivalModel> args )
    {
        SelectedFestival = args.Data;

        // Clone the selected row, to compare changed values with the original values
        SelectedFestivalOriginal = new FestivalModel
        {
            FestivalId = args.Data.FestivalId,
            Festival = args.Data.Festival,
            Festivaldatum = args.Data.Festivaldatum,
            StartInschrijving = args.Data.StartInschrijving,
            EindeInschrijving = args.Data.EindeInschrijving,
            Wachtlijst = args.Data.Wachtlijst,
            PubliceerPlanning = args.Data.PubliceerPlanning,
            MinutenTussenOptredens = args.Data.MinutenTussenOptredens,
            MaximumMinutenTussenOptredens = args.Data.MaximumMinutenTussenOptredens,
            MaximumUrenVrijwilligers = args.Data.MaximumUrenVrijwilligers,
            BoeteOnderbrekingOptredens = args.Data.BoeteOnderbrekingOptredens,
            StartVrijwilligersTaken = args.Data.StartVrijwilligersTaken,
            EindeVrijwilligersTaken = args.Data.EindeVrijwilligersTaken,
            StartVrijwilligersPauze = args.Data.StartVrijwilligersPauze,
            EindeVrijwilligersPauze = args.Data.EindeVrijwilligersPauze,
            EindeVasteVrijwilligersTaken = args.Data.EindeVasteVrijwilligersTaken,
            Aktief = args.Data.Aktief
        };

        StateHasChanged();
    }

    protected async Task SaveFestival()
    {
        if ( SelectedFestival is null )
            return;

        // Check the differences between the original and cjhanged version
        var differences = ObjectDiffHelper.GetDifferences(SelectedFestivalOriginal, SelectedFestival);

        if ( differences.Count > 0 )
        {
            string festivalName = SelectedFestival.Festival;

            foreach ( var diff in differences )
            {
                string logMessage =
                $"<_userName> heeft {diff.PropertyName} van editie {festivalName} gewijzigd van '{diff.OldValue}' in '{diff.NewValue}'.";

                await LoggingService.WriteUserActionFestivalAsync( SelectedFestival.FestivalId, "Beheer", "Festivals", "updated", logMessage );
            }
        }


        await FestivalService.ModifyFestivalAsync( SelectedFestival );
        await FestivalService.ModifyConditionAsync( SelectedFestival );

        Festivals = await FestivalService.GetFestivalDataAsync();
        await GridRef.Refresh();
        await GridRef.SelectRowAsync( Festivals.IndexOf( SelectedFestival ) );
    }

    protected async Task AddNewFestival()
    {
        if ( SelectedFestival is null )
            return;

        var _latestFestivalYear = await FestivalService.GetLatestFestivalAsync();

        var newFestival =  _latestFestivalYear + 1;
        DateTime startSubscription = new(_latestFestivalYear, 9, 15, 0, 0, 0);
        DateTime endSubscription = new (_latestFestivalYear + 1, 3, 31, 0, 0, 0);

        // What is the first Saterday of June for that year
        DateOnly firstSaterday = Enumerable.Range(1, 7)
        .Select(newDate => new DateOnly(newFestival, 6, newDate))
        .First(d => d.DayOfWeek == DayOfWeek.Saturday);

        // Insert new record for this year in the table, and get the Festival-Id
        var festivalId = await FestivalService.InsertNewFestivalAsync( firstSaterday, startSubscription, endSubscription );

        // Insert a new record in planner_conditions table with new festivalId
        var record = await FestivalService.InsertNewConditionsAsync(festivalId);

        var logMessage = $"<_userName> heeft een nieuwe festival editie ({newFestival}) toegevoegd.";

        await LoggingService.WriteUserActionFestivalAsync( festivalId, "Beheer", "Festivals", "added", logMessage );

        // refresh the table
        var festivalModels = await FestivalService.GetFestivalDataAsync();

        Festivals = await FestivalService.GetFestivalDataAsync();
        SelectedFestival = Festivals
            .OrderByDescending( f => int.Parse( f.Festival ) ) // Festival is een string
            .FirstOrDefault();
        await GridRef.Refresh();
    }

    protected async Task DeleteFestival()
    {
        if ( SelectedFestival is null )
            return;

        var deleteFestival = SelectedFestival.Festival;

        await FestivalService.DeleteConditionAsync( SelectedFestival.FestivalId );
        await FestivalService.DeleteFestivalAsync( SelectedFestival.FestivalId );

        var logMessage = $"<_userName> heeft een, nog niet gebruikte, festival editie ({deleteFestival}) verwijderd.";
        await LoggingService.WriteUserActionFestivalAsync( SelectedFestival.FestivalId, "Beheer", "Festivals", "deleted", logMessage );

        // refresh the table
        var festivalModels = await FestivalService.GetFestivalDataAsync();

        Festivals = await FestivalService.GetFestivalDataAsync();
        SelectedFestival = Festivals
            .OrderByDescending( f => int.Parse( f.Festival ) ) // Festival is een string
            .FirstOrDefault();
        await GridRef.Refresh();
    }
}