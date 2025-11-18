using Amusing.Models;
using Amusing.Services;

using Microsoft.AspNetCore.Components;

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

    private SfGrid<LogModel>? _gridLog;
    private List<LogModel> _loggingList = [];
    private List<DashboardStatisticsTotal> _totals = [];
    private List<DashboardStatisticsGenre> _genre = [];
    private List<DashboardStatisticsCountry> _country = [];
    private List<DashboardStatisticsStage> _stage = [];
    private List<IDictionary<string, object>> _pivot = [];
    private List<DashboardStatisticsGraph> _graph = [];
    private List<string> _pivotColumns = [];
    protected List<Edition> Editions = [];
    protected int selectedYears = 5;
    public int SelectedYears
    {
        get => selectedYears;
        set
        {
            if ( selectedYears == value )
                return;
            selectedYears = value;
            // Trigger synchronous wrapper that calls async loader
            _ = OnYearsChangedAsync( value );
        }
    }
    protected List<int> Years = [2, 5, 10 ];
    protected string? selectedEditionId;
    public string SelectedEditionId
    {
        get => selectedEditionId;
        set
        {
            if ( selectedEditionId == value )
                return;
            selectedEditionId = value;
            // Trigger synchronous wrapper that calls async loader
            _ = OnSelectedEditionChangedAsync( value );
        }
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

        _graph = await DashboardService.GetGraphDataAsync( SelectedYears);

        if ( Editions.Any() )
        {
            // Auto select the current festival edition
            SelectedEditionId = Editions
                .OrderByDescending( e => int.Parse( e.Text ) )
                .First().ID;

            if ( SelectedEditionId != null )
            {
                var _subscriptions = await DashboardService.GetNumberOfSubscriptions( int.Parse( SelectedEditionId ) );
                if ( _subscriptions > 0 )
                {
                    _totals = await DashboardService.GetDashboardStatisticsTotalsAsync( int.Parse( SelectedEditionId ) );
                    _genre = await DashboardService.GetDashboardStatisticsGenreAsync( int.Parse( SelectedEditionId ) );
                    _country = await DashboardService.GetDashboardStatisticsCountryAsync( int.Parse( SelectedEditionId ) );
                    _stage = await DashboardService.GetDashboardStatisticsStageAsync( int.Parse( SelectedEditionId ) );
                    _pivot = await DashboardService.GetSubscriptionsPivotAsync( int.Parse( SelectedEditionId ) );

                    if ( _pivot.Any() )
                        _pivotColumns = [ .. _pivot.First()
                            .Keys
                            .Where( k => k != "DeelnemersCategorie" ) ];

                    _hasData = true;

                    StateHasChanged();
                }
                else
                {
                    _hasData = false;
                }
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

    private async void AddSearchIcon()
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

        // First check: does it have subscriptions?
        var subs = await DashboardService.GetNumberOfSubscriptions(editionInt);
        if ( subs <= 0 )
        {
            _hasData = false;
            await InvokeAsync( StateHasChanged );
            return;
        }

        // Load all datasets
        _totals = await DashboardService.GetDashboardStatisticsTotalsAsync( editionInt );
        _genre = await DashboardService.GetDashboardStatisticsGenreAsync( editionInt );
        _country = await DashboardService.GetDashboardStatisticsCountryAsync( editionInt );
        _stage = await DashboardService.GetDashboardStatisticsStageAsync( editionInt );
        _pivot = await DashboardService.GetSubscriptionsPivotAsync( editionInt );

        if ( _pivot.Any() )
        {
            _pivotColumns = _pivot.First()
                .Keys
                .Where( k => k != "DeelnemersCategorie" )
                .ToList();
        }

        _hasData = true;

        await InvokeAsync( StateHasChanged );
    }

    protected async Task OnYearsChangedAsync( int years )
    {
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
            var _subscriptions = await DashboardService.GetNumberOfSubscriptions( int.Parse( SelectedEditionId ) );
            if ( _subscriptions > 0 )
            {
                _totals = await DashboardService.GetDashboardStatisticsTotalsAsync( int.Parse( SelectedEditionId ) );
                _genre = await DashboardService.GetDashboardStatisticsGenreAsync( int.Parse( SelectedEditionId ) );
                _country = await DashboardService.GetDashboardStatisticsCountryAsync( int.Parse( SelectedEditionId ) );
                _stage = await DashboardService.GetDashboardStatisticsStageAsync( int.Parse( SelectedEditionId ) );
                _pivot = await DashboardService.GetSubscriptionsPivotAsync( int.Parse( SelectedEditionId ) );

                if ( _pivot.Any() )
                    _pivotColumns = [ .. _pivot.First()
                            .Keys
                            .Where( k => k != "DeelnemersCategorie" ) ];

                _hasData = true;

                StateHasChanged();
            }
            else
            {
                _hasData = false;
            }
        }
        else
        { _hasData = false; }
    }
}