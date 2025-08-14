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
}
