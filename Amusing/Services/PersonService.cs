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
    public Task<List<PersonModel>> GetAllPersonsAsync()
    {
        return _dataService.ExecuteQueryAsync(QueryDefinitions.GetAllPersons,
            reader => new PersonModel
            {
                PersonId = Convert.ToUInt32(reader["PersonId"]),
                Name = reader["Name"].ToString(),
                PersonsEmail = reader["Email"]?.ToString(),
                Roles = reader["Roles"]?.ToString(),
                Volunteer = reader["Volunteer"]?.ToString(),
                FirstName = reader["FirstName"].ToString(),
                NameInfix = reader["NameInfix"].ToString(),
                LastName = reader["LastName"].ToString(),
                Address = reader["Address"].ToString(),
                Street = reader["Street"].ToString(),
                HomeNr = reader["HomeNr"].ToString(),
                HomeNrAddition = reader["HomeNrAddition"].ToString(),
                Zip = reader["Zip"].ToString(),
                City = reader["City"].ToString(),
                Mobile = reader["Mobile"].ToString(),
                Phone = reader["Phone"].ToString(),
                Active = Convert.ToInt32(reader["Active"])
            });
    }
}
