using Amusing.Helpers;
using Amusing.Models;

namespace Amusing.Services;

public class FestivalService( GenericDataService dataService )
{
    private readonly GenericDataService _dataService = dataService;

    public Task<List<FestivalModel>> GetFestivalOverviewAsync()
    {
        return _dataService.ExecuteQueryAsync( QueryDefinitions.GetFestivals,
    reader => new FestivalModel
    {
        FestivalId = Convert.ToUInt32( reader [ "festival_id" ] ),
        Festival = reader [ "Festival" ].ToString(),
        Datum = DateOnly.FromDateTime( reader.GetDateTime( reader.GetOrdinal( "Datum" ) ) ),
        Gepubliceerd = reader [ "Gepubliceerd" ].ToString()
    } );
    }
}