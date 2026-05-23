using System.Globalization;

using Amusing.Helpers;
using Amusing.Models;

namespace Amusing.Services;

public class GroupService( GenericDataService dataService )
{
    private readonly GenericDataService _dataService = dataService;

    public Task<List<GroupModel>> GetAllGroupsAsync()
    {
        return _dataService.ExecuteQueryAsync( QueryDefinitions.GetAllGroups, reader =>
        {
            return new GroupModel
            {
                GroupId = Convert.ToUInt16( reader [ "GroupId" ] ),
                Name = reader [ "Name" ]?.ToString(),
                GenreId = Convert.ToUInt16( reader [ "GenreId" ] ),
                Genre = reader [ "Genre" ].ToString(),
                City = reader [ "City" ].ToString(),
                CountryId = reader [ "CountryId" ].ToString()?.ToLower(),
                Country = CultureInfo.CurrentCulture.TextInfo.ToTitleCase( reader [ "Country" ].ToString()?.ToLower() ?? "" ),
                Website = reader [ "Website" ].ToString(),
                Email = reader [ "Email" ].ToString(),
                Photo = reader [ "Photo" ] != DBNull.Value ? ( byte [ ] ) reader [ "Photo" ] : null,
                Logo = reader [ "Logo" ] != DBNull.Value ? ( byte [ ] ) reader [ "Logo" ] : null,
                Description = reader [ "Description" ].ToString(),
                BankAccount = reader [ "BankAccount" ].ToString(),
                Active = Convert.ToInt16( reader [ "Active" ] ),
            };
        } );
    }
    public Task<List<GroupModel>> GetInactiveGroupsAsync()
    {
        return _dataService.ExecuteQueryAsync( QueryDefinitions.GetInactiveGroups, reader =>
        {
            return new GroupModel
            {
                GroupId = Convert.ToUInt16( reader [ "GroupId" ] ),
                Name = reader [ "Name" ]?.ToString(),
                GenreId = Convert.ToUInt16( reader [ "GenreId" ] ),
                Genre = reader [ "Genre" ].ToString(),
                City = reader [ "City" ].ToString(),
                CountryId = reader [ "CountryId" ].ToString()?.ToLower(),
                Country = CultureInfo.CurrentCulture.TextInfo.ToTitleCase( reader [ "Country" ].ToString()?.ToLower() ?? "" ),
                Website = reader [ "Website" ].ToString(),
                BankAccount = reader [ "BankAccount" ].ToString(),
                Active = Convert.ToInt16( reader [ "Active" ] ),
            };
        } );
    }
    public async Task<uint> AddGroupAsync( GroupModel model )
    {
        string description = string.IsNullOrEmpty(model.Description) ? "" : model.Description;

        Dictionary<string, object> parameters = new()
        {
            { "@Name", model.Name },
            { "@GenreId", model.GenreId },
            { "@City", model.City },
            { "@CountryId", model.CountryId },
            { "@Website", model.Website },
            { "@Description", description },
            { "@BankAccount", model.BankAccount },
            { "@Active", model.Active },
            { "@Photo", model.Photo },
            { "@Logo", model.Logo }
        };

        return await _dataService.ExecuteScalarAsync<uint>( QueryDefinitions.AddNewGroup, parameters );
    }
    public async Task<uint> AddGroupDetailsAsync( GroupModel model, uint groupId )
    {
        Dictionary<string, object> parameters = new()
        {
            { "@GroupId", groupId },
            { "@Email", model.Email }
        };

        return await _dataService.ExecuteScalarAsync<uint>( QueryDefinitions.AddNewGroupDetail, parameters );
    }
    public async Task UpdateGroupAsync( GroupModel model )
    {
        string description = string.IsNullOrEmpty(model.Description) ? "" : model.Description;

        Dictionary<string, object> parameters = new()
        {
            { "@GroupId", model.GroupId },
            { "@Name", model.Name },
            { "@GenreId", model.GenreId },
            { "@City", model.City },
            { "@CountryId", model.CountryId },
            { "@Website", model.Website },
            { "@Description", description },
            { "@BankAccount", model.BankAccount },
            { "@Active", model.Active },
            { "@Photo", model.Photo },
            { "@Logo", model.Logo }
        };

        await _dataService.ExecuteNonQueryAsync( QueryDefinitions.ModifyGroupByGroupId, parameters );
    }
    public async Task UpdateGroupDetailsAsync( GroupModel model )
    {
        Dictionary<string, object> parameters = new()
    {
        { "@GroupId", model.GroupId },
        { "@Email", model.Email }
    };

        await _dataService.ExecuteNonQueryAsync( QueryDefinitions.ModifyGroupDetailsByGroupId, parameters );
    }
    public async Task DeleteGroupAsync( uint groupId )
    {
        Dictionary<string, object> parameters = new()
        {
            { "@GroupId", groupId },
            { "@Active", 0 }
        };

        await _dataService.ExecuteNonQueryAsync( QueryDefinitions.DeleteGroupByGroupId, parameters );
    }
    public async Task DestroyGroupAsync( uint groupId )
    {
        Dictionary<string, object> parameters = new()
        {
            { "@GroupId", groupId },
            { "@Name", "Groep verwijderd" },
            { "@GenreId", 0 },
            { "@City", string.Empty },
            { "@CountryId", string.Empty },
            { "@Website", string.Empty },
            { "@Description", string.Empty },
            { "@BankAccount", string.Empty },
            { "@Active", 0 },
            { "@Photo", string.Empty },
            { "@Logo", string.Empty }
        };

        await _dataService.ExecuteNonQueryAsync( QueryDefinitions.ModifyGroupByGroupId, parameters );
    }
    public async Task ReactivateGroupAsync( uint groupId )
    {
        Dictionary<string, object> parameters = new()
        {
            { "@GroupId", groupId },
            { "@Active", 1 }
        };

        await _dataService.ExecuteNonQueryAsync( QueryDefinitions.ReactivateGroupByGroupId, parameters );
    }
}
