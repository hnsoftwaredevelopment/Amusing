using Amusing.Models;
using Amusing.Services;

using Microsoft.AspNetCore.Components;

using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Grids.Internal;
using Syncfusion.Blazor.Inputs;

namespace Amusing.Components.Pages;

public partial class Home : ComponentBase
{
    private bool _isLoading = false;
    private bool _firstRenderDone = false;

    private readonly int _festivalId = 20;

    protected string FileName = "Log bestanden";

    private SfGrid<LogModel>? _gridLog;
    private List<LogModel> _loggingList = [];
    private List<DashboardStatisticsTotal> _totals = [];
    private List<DashboardStatisticsGenre> _genre = [];
    private List<DashboardStatisticsCountry> _country = [];
    private List<DashboardStatisticsStage> _stage = [];
    private List<DashboardSubscriptionsPivot> _pivot = [];
    private List<string> _pivotColumns = [];

    [Inject]
    private LoggingService _loggingService { get; set; } = default!;

    [Inject]
    private DashboardService DashboardService { get; set; } = default!;

    public List<string> ToolbarItems = ["Zoek"];

    SfTextBox searchBox { get; set; }

    protected override async Task OnInitializedAsync()
    {
        _isLoading = true;

        _loggingList = await _loggingService.GetUserLoginsAsync();

        _isLoading = false;

        _totals = await DashboardService.GetDashboardStatisticsTotalsAsync( _festivalId );
        _genre = await DashboardService.GetDashboardStatisticsGenreAsync( _festivalId );
        _country = await DashboardService.GetDashboardStatisticsCountryAsync( _festivalId );
        _stage = await DashboardService.GetDashboardStatisticsStageAsync( _festivalId );
        _pivot = await DashboardService.GetSubscriptionsPivotAsync( _festivalId );

        if ( _pivot.Any() )
            _pivotColumns = _pivot.First().Podia.Keys.ToList();

        StateHasChanged();
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
}