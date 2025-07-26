using Amusing.Helpers;
using Amusing.Models;

namespace Amusing.Services;

public class PersonService( GenericDataService dataService )
{
    private readonly GenericDataService _dataService = dataService;

    public Task<List<PersonOverviewModel>> GetPersonOverviewAsync()
    {
        return _dataService.ExecuteQueryAsync( QueryDefinitions.GetPersonsOverview,
            reader => new PersonOverviewModel
            {
                PersoonId = Convert.ToUInt32( reader [ "PersonId" ] ),
                Naam = reader [ "Name" ].ToString(),
                Email = reader [ "Email" ].ToString(),
                Rollen = reader [ "Role" ]?.ToString()
                         ?.Split( ", ", StringSplitOptions.RemoveEmptyEntries )
                         .ToList() ?? [ ],
                Vrijwilliger = reader [ "Volunteer" ]?.ToString()
                         ?.Split( ", ", StringSplitOptions.RemoveEmptyEntries )
                         .ToList() ?? [ ]
            } );
    }
}
