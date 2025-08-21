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
                PersonId = Convert.ToUInt16( reader [ "PersonId" ] ),
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
                PersonId = Convert.ToUInt16( reader [ "PersonId" ] ),
                Name = reader [ "Name" ].ToString(),
                Email = reader.IsDBNull( reader.GetOrdinal( "Email" ) )
                    ? string.Empty
                    : reader [ "Email" ]?.ToString(),
                GroupName = reader.IsDBNull( reader.GetOrdinal( "GroupNames" ) )
                    ? string.Empty
                    : reader [ "GroupNames" ]?.ToString()
            }, parameters );
    }
    public async Task ModifyPersonRoleAsync( uint groupId, uint personId, string role )
    {
        Dictionary<string, object> parameters = new()
        {
            { "@GroupId", groupId },
            { "@PersonId", personId },
            { "@Role", role }
        };

        await _dataService.ExecuteNonQueryAsync( QueryDefinitions.ModifyPersonRole, parameters );
    }
    public async Task InsertNewPersonRoleAsync( uint groupId, uint personId, string role )
    {
        Dictionary<string, object> parameters = new()
    {
        { "@GroupId", groupId },
        { "@PersonId", personId },
        { "@Role", role }
    };

        await _dataService.ExecuteNonQueryAsync( QueryDefinitions.InsertNewPersonRole, parameters );
    }
    public async Task DeletePersonRoleAsync( uint groupId, uint personId )
    {
        Dictionary<string, object> parameters = new()
        {
            { "@GroupId", groupId },
            { "@PersonId", personId }
        };

        await _dataService.ExecuteNonQueryAsync( QueryDefinitions.DeletePersonRole, parameters );
    }
}
