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

public partial class ListGroups : ComponentBase
{
    [Inject] protected LoggingService LoggingService { get; set; } = default!;
    [Inject] protected RegistrationService RegistrationService { get; set; } = default!;

    protected int CurrentFestivalYear;
    protected int maxslider;
    protected int minslider;
    protected string FileName = "Groepen";
    protected SfGrid<FestivalParticipationDynamicViewModel> GridRef;
    protected bool FilterOutOldGroups = false;
    protected bool IsLoading = false;
    protected int VisibleRowCount = 0;
    protected List<FestivalParticipationDynamicViewModel> Zanggroepen = [];
    protected List<int> YearColumns = [];

    protected int FilterOnFestival;

    // For the selectable display of the Number of columns
    protected int _showNumberOfYears = 10;

    protected List<int> VisibleYearCollumns => YearColumns
        .TakeLast( ShowNumberOfYears == int.MaxValue ? YearColumns.Count : ShowNumberOfYears )
        .ToList();

    protected int ShowNumberOfYears
    {
        get => _showNumberOfYears;
        set
        {
            if ( _showNumberOfYears != value )
            {
                _showNumberOfYears = value;
                StateHasChanged();
            }
        }
    }

    protected int _slidervalue;
    protected int slidervalue
    {
        get => _slidervalue;
        set
        {
            if ( _slidervalue != value )
            {
                _slidervalue = value;
                ShowNumberOfYears = maxslider - _slidervalue + 1;

                _ = LoadDataAsync();
            }
        }
    }

    protected override async Task OnInitializedAsync()
    {
        CurrentFestivalYear = await RegistrationService.GetCurrentFestivalYearAsync();
        maxslider = CurrentFestivalYear;
        minslider = maxslider - 15;
        slidervalue = maxslider - 10;

        FilterOnFestival = await RegistrationService.GetCurrentFestivalYearAsync() - 3; // This Value should be equal to NumberOfYearsForExclusion in QueryDefinitions.GetFestivalOverviewQuery

        await LoadDataAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            if (Zanggroepen?.Count > 0)
            {
                await UpdateVisibleRowCountAsync();
            }
        }
    }

    protected async Task LoadDataAsync()
    {
        IsLoading = true;
        StateHasChanged();
        Zanggroepen = await RegistrationService.GetRegistrationdPerFestivalAsync( filterOutOldGroups: FilterOutOldGroups );

        YearColumns = Zanggroepen
            .SelectMany( z => z.DeelnamePerJaar.Keys )
            .Union( Enumerable.Range( minslider, maxslider - minslider + 1 ) )
            .Distinct()
            .OrderBy( y => y )
            .ToList();

        //Filter only rows with year values
        var zichtbareJaren = VisibleYearCollumns;
        Zanggroepen = Zanggroepen
            .Where( z => z.DeelnamePerJaar
                .Any( d => zichtbareJaren.Contains( d.Key ) && !string.IsNullOrWhiteSpace( d.Value ) ) )
            .ToList();

        IsLoading = false;

        if ( GridRef is not null )
        {
            await GridRef.Refresh();
        }
    }

    protected async Task OnFilterChanged( bool value )
    {
        FilterOutOldGroups = value;
        await LoadDataAsync();
    }

    // Manage direct search functionality
    public async void OnInput( InputEventArgs args )
    {
        await GridRef.SearchAsync( args.Value );

        await Task.Delay( 50 );
        await UpdateVisibleRowCountAsync();
    }

    // Export functions
    protected async Task ExportToExcel()
    {
        var exportProps = new ExcelExportProperties
        {
            FileName = $"{FileName}-{DateTime.Now:yyyyMMdd}-{DateTime.Now:HHmm}.xlsx"
        };

        await GridRef!.ExportToExcelAsync( exportProps );

        string _report = $"<_userName> heeft \"{exportProps.FileName}\" geexporteerd";
        await LoggingService.WriteUserActionAsync( "Lijsten", "Groepen", "success", _report );
    }

    protected async Task ExportToCsv()
    {
        var exportProps = new ExcelExportProperties
        {
            FileName = $"{FileName}-{DateTime.Now:yyyyMMdd}-{DateTime.Now:HHmm}.csv"
        };

        await GridRef!.ExportToCsvAsync( exportProps );

        string _report = $"<_userName> heeft \"{exportProps.FileName}\" geexporteerd";
        await LoggingService.WriteUserActionAsync( "Lijsten", "Groepen", "success", _report );
    }

    protected async Task ExportToPdf()
    {
        var exportProps = new PdfExportProperties
        {
            FileName = $"{FileName}-{DateTime.Now:yyyyMMdd}-{DateTime.Now:HHmm}.pdf",
            PageOrientation = PageOrientation.Landscape,
            PageSize=PdfPageSize.A4,
            AllowHorizontalOverflow = true
        };
        await GridRef!.ExportToPdfAsync( exportProps );

        string _report = $"<_userName> heeft \"{exportProps.FileName}\" geexporteerd";
        await LoggingService.WriteUserActionAsync( "Lijsten", "Groepen", "success", _report );
    }

    protected async Task OnSliderChanged( double newValue )
    {
        slidervalue = ( int ) newValue;
        ShowNumberOfYears = maxslider - ( int ) newValue + 1;
        StateHasChanged();
    }

    protected async Task OnGridDataBound()
    {
        await UpdateVisibleRowCountAsync();
    }

    protected async Task OnGridActionComplete( ActionEventArgs<FestivalParticipationDynamicViewModel> args )
    {
        if ( args.RequestType == Syncfusion.Blazor.Grids.Action.Refresh ||
        args.RequestType == Syncfusion.Blazor.Grids.Action.Filtering ||
        args.RequestType == Syncfusion.Blazor.Grids.Action.Paging ||
        args.RequestType == Syncfusion.Blazor.Grids.Action.Sorting )
        {
            await UpdateVisibleRowCountAsync();
        }
    }

    protected async Task UpdateVisibleRowCountAsync()
    {
        if (GridRef == null)
            return;

        var records = await GridRef.GetCurrentViewRecordsAsync();
        VisibleRowCount = records?.Count ?? 0;
        StateHasChanged();
    }
}