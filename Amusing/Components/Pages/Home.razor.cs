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
    private List<string> _pivotColumns = [];
    protected List<Edition> Editions = [];
    protected string? SelectedEditionId;

    [Inject]
    private LoggingService _loggingService { get; set; } = default!;

    [Inject]
    private DashboardService DashboardService { get; set; } = default!;

    [Inject]
    private EditionService EditionService { get; set; } = default!;

    public List<string> ToolbarItems = ["Zoek"];

    SfTextBox searchBox { get; set; }
    SfComboBox<string, Edition> selectEdition { get; set; }

    protected override async Task OnInitializedAsync()
    {
        _isLoading = true;

        _loggingList = await _loggingService.GetUserLoginsAsync();

        _isLoading = false;

        Editions = await EditionService.GetEditionsAsync();

        if ( Editions.Any() )
        {
            // Auto select the current festival edition
            SelectedEditionId = Editions
                .OrderByDescending( e => int.Parse( e.Text ) )
                .First().ID;
            selectEdition.Value = SelectedEditionId;

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