using Amusing.Helpers;
using Amusing.Models;

namespace Amusing.Services;

public class RegistrationService( GenericDataService dataService )
{
    private readonly GenericDataService _dataService = dataService;

    public Task<List<RegistrationModel>> GetRegistrationsByFestivalIdAsync( uint festivalId )
    {
        return _dataService.ExecuteQueryAsync(
            QueryDefinitions.GetRegistrationsByFestifalId,
            reader => new RegistrationModel
            {
                FestivalId = Convert.ToUInt32( reader [ "festival_id" ] ),
                Datum = Convert.ToDateTime( reader [ "Datum" ] ),
                Naam = reader [ "Naam" ].ToString(),
                Stad = reader [ "Stad" ].ToString(),
                Podium = reader [ "Podium" ].ToString(),
                Zangers = Convert.ToInt32( reader [ "Zangers" ] ),
                Genre = reader [ "Genre" ].ToString(),
                TeBetalen = Convert.ToDecimal( reader [ "TeBetalen" ] ),
                Betaald = reader [ "Betaald" ].ToString(),
                Bevestigd = reader [ "Bevestigd" ].ToString(),
                Kleedkamer = reader [ "Kleedkamer" ].ToString(),
                Binnen = Convert.ToInt32( reader [ "Binnen" ] ),
                Buiten = Convert.ToInt32( reader [ "Buiten" ] )
            },
            new Dictionary<string, object> { { "@festivalId", festivalId } }
            );
    }
}
