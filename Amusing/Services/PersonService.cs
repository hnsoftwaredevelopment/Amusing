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

    public Task<List<PersonModel>> GetAllActivePersonsByGroupId( uint groupId )
    {
        Dictionary<string, object> parameters = new()
        {
            { "@GroupId", groupId }
        };
        return _dataService.ExecuteQueryAsync( QueryDefinitions.GetAllActivePersonsByGroupId,
            reader => new PersonModel
            {
                PersoonId = Convert.ToUInt16( reader [ "PersonId" ] ),
                Name = reader [ "Name" ].ToString(),
                Email = reader [ "Email" ].ToString(),
                GroupId = Convert.ToUInt16( reader [ "GroupId" ] ),
                Active = Convert.ToInt16( reader [ "Active" ] ),
                Role = reader [ "Role" ].ToString(),
            }, parameters );
    }

    public Task<List<PersonModel>> GetAllInactivePersonsByGroupId( uint groupId )
    {
        Dictionary<string, object> parameters = new()
    {
        { "@GroupId", groupId }
    };
        return _dataService.ExecuteQueryAsync( QueryDefinitions.GetAllInactivePersonsByGroupId,
            reader => new PersonModel
            {
                PersoonId = Convert.ToUInt16( reader [ "PersonId" ] ),
                Name = reader [ "Name" ].ToString(),
                Email = reader [ "Email" ].ToString(),
                GroupId = Convert.ToUInt16( reader [ "GroupId" ] ),
                Active = Convert.ToInt16( reader [ "Active" ] ),
                Role = reader [ "Role" ].ToString(),
            }, parameters );
    }

    public Task<List<PersonModel>> GetAllUnrelatedPersonsByGroupId( uint groupId )
    {
        Dictionary<string, object> parameters = new()
    {
        { "@GroupId", groupId }
    };
        return _dataService.ExecuteQueryAsync( QueryDefinitions.GetAllUnrelatedPersonsByGroupId,
            reader => new PersonModel
            {
                PersoonId = Convert.ToUInt16( reader [ "PersonId" ] ),
                Name = reader [ "Name" ].ToString(),
                Email = reader [ "Email" ].ToString(),
                GroupId = Convert.ToUInt16( reader [ "GroupId" ] ),
                Active = Convert.ToInt16( reader [ "Active" ] ),
                Role = reader [ "Role" ].ToString(),
            }, parameters );
    }
}
