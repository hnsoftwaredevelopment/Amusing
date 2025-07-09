using System.Data;
using System.Data.Common;

using Amusing.Models;

using Microsoft.AspNetCore.Authentication;

using MySqlCommand = MySql.Data.MySqlClient.MySqlCommand;
using MySqlConnection = MySql.Data.MySqlClient.MySqlConnection;

namespace Amusing.Services;

public class CustomAuthenticationService
{
    private readonly IConfiguration _configuration;

    public CustomAuthenticationService( IConfiguration configuration )
    {
        _configuration = configuration;
    }

    public async Task<LoginModel?> ValidateUserAsync( string username, string password )
    {
        string? connectionString = _configuration.GetConnectionString("DefaultConnection");

        using MySqlConnection connection = new(connectionString);
        await connection.OpenAsync();

        string hashedPassword = ComputeMd5Hash(password);

        using MySqlCommand command = new(
            "SELECT user_id, username, role FROM AH_Beheer WHERE username = @username AND password = @password",
            connection);

        command.Parameters.AddWithValue( "@username", username );
        command.Parameters.AddWithValue( "@password", hashedPassword );

        using DbDataReader reader = await command.ExecuteReaderAsync();

        if ( await reader.ReadAsync() )
        {
            return new LoginModel
            {
                UserId = reader.GetInt32( "user_id" ),
                Username = reader.GetString( "username" ),
                Role = reader.GetString( "role" ).Trim().ToLowerInvariant()
            };
        }

        return null;
    }

    private string ComputeMd5Hash( string input )
    {
        using System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
        byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
        byte[] hashBytes = md5.ComputeHash(inputBytes);
        return BitConverter.ToString( hashBytes ).Replace( "-", "" ).ToLowerInvariant();
    }
}