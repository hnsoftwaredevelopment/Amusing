using Amusing.Helpers;
using Amusing.Models;

namespace Amusing.Services;

public class VolunteerService( GenericDataService dataService )
{
    private readonly GenericDataService _dataService = dataService;

    public Task<List<VolunteerModel>> GetVolunteersByFestivalIdAsync( uint festivalId )
    {
        return _dataService.ExecuteQueryAsync(
            QueryDefinitions.GetVolunteersByFestivalId,
            reader => new VolunteerModel
            {
                FestivalId = Convert.ToUInt32( reader [ "festival_id" ] ),
                Datum = Convert.ToDateTime( reader [ "Datum" ] ),
                Naam = reader [ "Naam" ].ToString(),
                Van = reader [ "Van" ].ToString(),
                Tot = reader [ "Tot" ].ToString(),
                Uren = Convert.ToInt32( reader [ "Uren" ] ),
                Lunch = reader [ "Lunch" ].ToString().ToLower(),
                Vegetarisch = reader [ "Vegetarisch" ].ToString().ToLower(),
                Bijeenkomst = reader [ "Bijeenkomst" ].ToString().ToLower(),
                Ervaring = reader [ "Ervaring" ].ToString().ToLower(),
                Podiumdienst = reader [ "Podiumdienst" ].ToString().ToLower(),
                Overige = reader [ "Overige" ].ToString().ToLower(),
                Afgehaakt = reader [ "Afgehaakt" ].ToString().ToLower(),
            },
            new Dictionary<string, object> { { "@festivalId", festivalId } }
        );
    }
}
