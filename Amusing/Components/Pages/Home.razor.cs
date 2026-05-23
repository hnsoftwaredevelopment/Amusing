using System.Globalization;

using Amusing.Models;
using Amusing.Services;

using Microsoft.AspNetCore.Components;

using Syncfusion.Blazor.Charts;
using Syncfusion.Blazor.DropDowns;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Grids.Internal;
using Syncfusion.Blazor.Inputs;

namespace Amusing.Components.Pages;

public partial class Home : ComponentBase
{
    private bool _isLoading = false;
    private bool _firstRenderDone = false;
    private bool _hasData = false;

    protected string FileName = "Log bestanden";
    private readonly string[] _dutchMonths = { "", "Jun", "Jul", "Aug", "Sep", "Okt", "Nov", "Dec", "Jan", "Feb", "Mrt", "Apr", "Mei" };
    private readonly string[] _seriesColors =
	[
		"#FF0000",
        "#8FB8DE",
        "#B7D3A8",
        "#E8C8A0",
        "#D6A5C4",
        "#FCF6BD",
        "#D0F4DE",
        "#F5EBE0",
        "#FFE5D9",
        "#C7C7C7",
    ];

    private SfGrid<LogModel>? _gridLog;
    private List<LogModel> _loggingList = [];
    private List<DashboardStatisticsTotal> _totals = [];
    private List<DashboardStatisticsGenre> _genre = [];
    private List<DashboardStatisticsCountry> _country = [];
    private List<DashboardStatisticsStage> _stage = [];
    private List<IDictionary<string, object?>> _pivot = [];
    private List<DashboardStatisticsGraph> _graph = [];
    private List<string> _pivotColumns = [];
    protected List<Edition> Editions = [];
    protected int selectedYears = 5;
    public int SelectedYears
    {
        get => selectedYears;
        set => selectedYears = value;
    }

    private double YAxisMax
    {
        get
        {
            if ( _graph == null || !_graph.Any() )
                return 20;

            var max = _graph.Max(x => x.Number);
            return Math.Ceiling( max / 20.0 ) * 20;
        }
    }

    private int MaxYear => _graph.Any() ? _graph.Max( x => x.FestivalId ) : 0; // of Festival als string -> int.Parse

    protected List<int> Years = [2, 5, 10 ];
    protected string? selectedEditionId;
    public string? SelectedEditionId
    {
        get => selectedEditionId;
        set => selectedEditionId = value;
    }


    [Inject]
    private LoggingService _loggingService { get; set; } = default!;

    [Inject]
    private DashboardService DashboardService { get; set; } = default!;

    [Inject]
    private EditionService EditionService { get; set; } = default!;

    public List<string> ToolbarItems = ["Zoek"];

    SfTextBox searchBox { get; set; }

    protected override async Task OnInitializedAsync()
    {
        _isLoading = true;

        _loggingList = await _loggingService.GetUserLoginsAsync();

        _isLoading = false;

        Editions = await EditionService.GetEditionsAsync();

        _graph = await DashboardService.GetGraphDataAsync( SelectedYears );

        if ( Editions.Any() )
        {
            // Auto select the current festival edition
            SelectedEditionId = Editions
                .OrderByDescending( e => int.Parse( e.Text ) )
                .First().ID;

            if ( SelectedEditionId != null )
            {
                await LoadSelectedEditionDashboardAsync( int.Parse( SelectedEditionId ) );
            }
        }
    }

    protected override async Task OnAfterRenderAsync( bool firstRender )
    {
        if ( !_firstRenderDone )
        {
            _firstRenderDone = true;

            // hier kun je eventueel initialisaties doen
            // of SignalR subscriptions starten
        }
    }

    public async Task OnInput( InputEventArgs args )
    {
        await _gridLog.SearchAsync( args.Value );

        await Task.Delay( 50 );
    }

    private async Task AddSearchIcon()
    {
        if ( searchBox != null )
        {
            await searchBox.AddIconAsync( "append", "fa fa-search" );
        }
    }

    private async Task OnSelectedEditionChangedAsync( string selectedId )
    {
        // Basic sanity check
        if ( string.IsNullOrWhiteSpace( selectedId ) )
        {
            _hasData = false;
            await InvokeAsync( StateHasChanged );
            return;
        }

        var editionInt = int.Parse(selectedId);

        await LoadSelectedEditionDashboardAsync( editionInt );
        await InvokeAsync( StateHasChanged );
    }

    protected async Task OnYearsChangedAsync( int years )
    {
        SelectedYears = years;
        _graph = await DashboardService.GetGraphDataAsync( years );
        await InvokeAsync( StateHasChanged );
    }

    protected async Task OnEditionChanged( string selectedId )
    {
        if ( string.IsNullOrWhiteSpace( selectedId ) )
            return;

        SelectedEditionId = selectedId;

        if ( SelectedEditionId != null )
        {
            await LoadSelectedEditionDashboardAsync( int.Parse( SelectedEditionId ) );
            StateHasChanged();
        }
        else
        { _hasData = false; }
    }

    private async Task LoadSelectedEditionDashboardAsync( int editionId )
    {
        var subscriptions = await DashboardService.GetNumberOfSubscriptions( editionId );
        if ( subscriptions <= 0 )
        {
            _hasData = false;
            _totals = [];
            _genre = [];
            _country = [];
            _stage = [];
            _pivot = [];
            _pivotColumns = [];
            return;
        }

        var totalsTask = DashboardService.GetDashboardStatisticsTotalsAsync( editionId );
        var genreTask = DashboardService.GetDashboardStatisticsGenreAsync( editionId );
        var countryTask = DashboardService.GetDashboardStatisticsCountryAsync( editionId );
        var stageTask = DashboardService.GetDashboardStatisticsStageAsync( editionId );
        var pivotTask = DashboardService.GetSubscriptionsPivotAsync( editionId );

        await Task.WhenAll( totalsTask, genreTask, countryTask, stageTask, pivotTask );

        _totals = await totalsTask;
        _genre = await genreTask;
        _country = await countryTask;
        _stage = await stageTask;
        _pivot = await pivotTask;

        _pivotColumns = _pivot.Any()
            ? [ .. _pivot.First().Keys.Where( k => k != "DeelnemersCategorie" ) ]
            : [];

        _hasData = true;
    }

    private void OnXAxisLabelRender( AxisLabelRenderEventArgs args )
    {
        if ( string.IsNullOrWhiteSpace( args?.Text ) )
            return;

        // Parse the numeric label (the MonthOrder)
        if ( int.TryParse( args.Text, out int monthOrder ) )
        {
            // Validate range and map to _dutchMonths
            if ( monthOrder >= 1 && monthOrder < _dutchMonths.Length )
            {
                args.Text = _dutchMonths [ monthOrder ];
                return;
            }
        }
    }
}
