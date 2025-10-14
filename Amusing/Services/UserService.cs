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
                Password = "",
                Role = reader [ "Role" ].ToString() ?? string.Empty,
                LastLoginDate = reader [ "LastLoginDate" ] == DBNull.Value
                    ? "nooit"
                    : Convert.ToDateTime( reader [ "LastLoginDate" ] ).ToString( "dd/MM/yyyy" )
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
            { "@Password", model.Password },
            { "@PasswordHash", model.PasswordHash }
            };

        await _dataService.ExecuteNonQueryAsync( QueryDefinitions.ModifyPasswordByUserId, parameters );
    }

    public async Task<uint> AddUserAsync( UserModel model )
    {
        Dictionary<string, object> parameters = new()
    {
        { "@UserName", model.Username },
        { "@Password", model.Password },
        { "@Role", model.Role }
    };

        return await _dataService.ExecuteScalarAsync<uint>( QueryDefinitions.AddNewUser, parameters );
    }

    public async Task DeleteUserAsync( UserModel model )
    {
        Dictionary<string, object> parameters = new()
    {
        { "@UserId", model.UserId }        };

        await _dataService.ExecuteNonQueryAsync( QueryDefinitions.DeleteUserByUserId, parameters );
    }
}
