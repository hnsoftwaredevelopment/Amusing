using System.Diagnostics.Eventing.Reader;
using System.Dynamic;

using Amusing.Helpers;
using Amusing.Models;

namespace Amusing.Services;

public class DashboardService ( GenericDataService dataService )
{
    private readonly GenericDataService _dataService = dataService;
    public Task<List<DashboardStatisticsTotal>> GetDashboardStatisticsTotalsAsync(int _festivalId)
    {
        return _dataService.ExecuteQueryAsync(
            QueryDefinitions.DashboardStatisticsTotal,
           reader => new DashboardStatisticsTotal
           {
               Total = Convert.ToInt32( reader [ "Total" ] ),
               InQueue = Convert.ToInt32( reader [ "InQueue" ] ),
               Paid = Convert.ToInt32( reader [ "Paid" ] ),
               DroppedOut = Convert.ToInt32( reader [ "DroppedOut" ] )
           },
           new Dictionary<string, object> { { "@FestivalId", _festivalId } }
            );
    }

    public Task<List<DashboardStatisticsGenre>> GetDashboardStatisticsGenreAsync( int _festivalId )
    {
        return _dataService.ExecuteQueryAsync(
            QueryDefinitions.DashboardStatisticsGenre,
           reader => new DashboardStatisticsGenre
           {
               Genre = reader [ "Genre" ].ToString() ?? string.Empty,
               Total = Convert.ToInt32( reader [ "Total" ] ),
               InQueue = Convert.ToInt32( reader [ "InQueue" ] ),
               Paid = Convert.ToInt32( reader [ "Paid" ] ),
               DroppedOut = Convert.ToInt32( reader [ "DroppedOut" ] )
           },
           new Dictionary<string, object> { { "@FestivalId", _festivalId } }
            );
    }

    public Task<List<DashboardStatisticsCountry>> GetDashboardStatisticsCountryAsync( int _festivalId )
    {
        return _dataService.ExecuteQueryAsync(
            QueryDefinitions.DashboardStatisticsCountry,
           reader => new DashboardStatisticsCountry
           {
               Country = reader [ "Country" ].ToString() ?? string.Empty,
               Total = Convert.ToInt32( reader [ "Total" ] ),
               InQueue = Convert.ToInt32( reader [ "InQueue" ] ),
               Paid = Convert.ToInt32( reader [ "Paid" ] ),
               DroppedOut = Convert.ToInt32( reader [ "DroppedOut" ] )
           },
           new Dictionary<string, object> { { "@FestivalId", _festivalId } }
            );
    }

    public Task<List<DashboardStatisticsStage>> GetDashboardStatisticsStageAsync( int _festivalId )
    {
        return _dataService.ExecuteQueryAsync(
            QueryDefinitions.DashboardStatisticsStagetype,
           reader => new DashboardStatisticsStage
           {
               Stagetype = reader [ "Stagetype" ].ToString() ?? string.Empty,
               Total = Convert.ToInt32( reader [ "Total" ] ),
               InQueue = Convert.ToInt32( reader [ "InQueue" ] ),
               Paid = Convert.ToInt32( reader [ "Paid" ] ),
               DroppedOut = Convert.ToInt32( reader [ "DroppedOut" ] )
           },
           new Dictionary<string, object> { { "@FestivalId", _festivalId } }
            );
    }

    public async Task<List<IDictionary<string, object>>> GetSubscriptionsPivotAsync( int festivalId )
    {
        var list = new List<IDictionary<string, object>>();

        await _dataService.ExecuteQueryAsync(
            QueryDefinitions.DashboardStatisticsSubscribtionsByNumberByStagetype,
            reader =>
            {
                IDictionary<string, object> row = new ExpandoObject();

                // Fixed column
                row [ "DeelnemersCategorie" ] = reader [ "DeelnemersCategorie" ]?.ToString() ?? string.Empty;

                // Dynamic columns    
                for ( int i = 0; i < reader.FieldCount; i++ )
                {
                    var colName = reader.GetName(i);

                    if ( colName != "DeelnemersCategorie" )
                    {
                        row [ colName ] = reader.IsDBNull( i ) ? 0 : Convert.ToInt32( reader [ i ] );
                    }
                }

                list.Add( row );
                return row;
            },
            new Dictionary<string, object>
            {
            { "@FestivalId", festivalId }
            }
        );

        return list;
    }

    public async Task<int> GetNumberOfSubscriptions( int festivalId )
    {
        var result = await _dataService.ExecuteScalarAsync<int>(
            QueryDefinitions.DashboardStatisticsGetNubmerOfSubscribtions,
            new Dictionary<string, object>
            {
                { "@FestivalId", festivalId }
            }
        );
        return result;
    }

    public async Task<List<DashboardStatisticsGraph>> GetGraphDataAsync( int years )
    {
        var result = new List<DashboardStatisticsGraph>();

        await _dataService.ExecuteQueryAsync<DashboardStatisticsGraph>(
            QueryDefinitions.DashboardStatisticsGetGraphData,
            reader =>
            {
                var item = new DashboardStatisticsGraph
            {
                FestivalId = Convert.ToInt32(reader["FestivalId"]),
                Festival = reader["Festival"]?.ToString() ?? string.Empty,
                Month = reader["Month"]?.ToString() ?? string.Empty,
                MonthOrder = Convert.ToInt32(reader["MonthOrder"]),
                Number = Convert.ToInt32(reader["Number"])
            };
                result.Add( item );
                return item;
            },
            new Dictionary<string, object> { { "@Years", years } }
        );

        // Bereken cumulatieve waarden per festival
        var cumulativeResult = result
        .GroupBy(g => g.Festival)
        .SelectMany(festivalGroup =>
        {
            int runningTotal = 0;
            return festivalGroup
                .OrderBy(g => g.MonthOrder)
                .Select(g =>
                {
                    runningTotal += g.Number;
                    return new DashboardStatisticsGraph
                    {
                        FestivalId = g.FestivalId,
                        Festival = g.Festival,
                        Month = g.Month,
                        MonthOrder = g.MonthOrder,
                        Number = runningTotal // cumulatief
                    };
                });
        })
        .ToList();

        return cumulativeResult;
    }
}
