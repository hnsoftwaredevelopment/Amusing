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

    public async Task<List<DashboardSubscriptionsPivot>> GetSubscriptionsPivotAsync( int festivalId )
    {
        var list = new List<DashboardSubscriptionsPivot>();

        await _dataService.ExecuteQueryAsync(
            QueryDefinitions.DashboardStatisticsSubscribtionsByNumberByStagetype,
            reader =>
            {
				var row = new DashboardSubscriptionsPivot
				{
					// Fixed column
					DeelnemersCategorie = reader [ "DeelnemersCategorie" ].ToString() ?? string.Empty
				};

				// Dynamic columns
				for ( int i = 0; i < reader.FieldCount; i++ )
                {
                    var colName = reader.GetName(i);

                    if ( colName != "DeelnemersCategorie" )
                    {
                        int value = reader.IsDBNull(i) ? 0 : Convert.ToInt32(reader[i]);
                        row.Podia [ colName ] = value;
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

}
