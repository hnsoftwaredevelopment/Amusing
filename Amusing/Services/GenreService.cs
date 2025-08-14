using Amusing.Helpers;
using Amusing.Models;

namespace Amusing.Services;

public class GenreService( GenericDataService dataService )
{

    private readonly GenericDataService _dataService = dataService;

    public Task<List<GenreModel>> GetGenresAsync()
    {
        return _dataService.ExecuteQueryAsync( QueryDefinitions.GetGenres, reader =>
        {
            return new GenreModel
            {
                GenreId = Convert.ToUInt16( reader [ "GenreId" ] ),
                Nl = reader [ "NL" ]?.ToString(),
                De = reader [ "DE" ]?.ToString(),
                En = reader [ "EN" ]?.ToString(),
            };
        } );
    }
}
