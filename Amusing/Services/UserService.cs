using Amusing.Helpers;
using Amusing.Models;

namespace Amusing.Services;

public class UserService( GenericDataService dataService )
{
    private readonly GenericDataService _dataService = dataService;
    public Task<List<UserModel>> GetAllUsersAsync()
    {
        return _dataService.ExecuteQueryAsync(
            QueryDefinitions.GetAllUsers,
            reader => new UserModel
            {
                UserId = Convert.ToUInt32( reader [ "UserId" ] ),
                Username = reader [ "UserName" ].ToString() ?? string.Empty,
                Password = reader [ "Password" ].ToString() ?? string.Empty,
                Role = reader [ "Role" ].ToString() ?? string.Empty
            } );
    }

    public async Task UpdateUserAsync( UserModel model )
    {
        Dictionary<string, object> parameters = new()
        {
            { "@UserId", model.UserId },
            { "@UserName", model.Username },
            { "@Role", model.Role }
            };

        await _dataService.ExecuteNonQueryAsync( QueryDefinitions.ModifyUserByUserId, parameters );
    }

    public async Task UpdatePasswordAsync( UserModel model )
    {
        Dictionary<string, object> parameters = new()
        {
            { "@UserId", model.UserId },
            { "@Password", model.Password }
            };

        await _dataService.ExecuteNonQueryAsync( QueryDefinitions.ModifyPasswordByUserId, parameters );
    }

    public async Task DeleteUserAsync( UserModel model )
    {
        Dictionary<string, object> parameters = new()
    {
        { "@UserId", model.UserId }        };

        await _dataService.ExecuteNonQueryAsync( QueryDefinitions.DeleteUserByUserId, parameters );
    }
}
