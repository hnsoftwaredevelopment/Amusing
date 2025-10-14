using System.Data;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

using Amusing.Models;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

using MySqlConnector;


namespace Amusing.Services;

public class CustomAuthenticationService
{
    private readonly IConfiguration _configuration;

    public CustomAuthenticationService( IConfiguration configuration )
    {
        _configuration = configuration;
    }

    public async Task<bool> LoginAsync( string username, string password )
    {
        var user = await ValidateUserAsync(username, password);
        return user != null;
        //if ( user == null )
        //    return false;

        // Build claims
    //    var claims = new List<Claim>
    //{
    //    new Claim(ClaimTypes.Name, user.Username),
    //    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
    //    new Claim(ClaimTypes.Role, user.Role)
    //};

    //    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    //    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

    //    // Create cookie
    //    var authProperties = new AuthenticationProperties
    //    {
    //        IsPersistent = true,
    //        ExpiresUtc = DateTime.UtcNow.AddHours(12)
    //    };

        //var httpContext = _httpContextAccessor.HttpContext;
        //await httpContext.SignInAsync(
        //    CookieAuthenticationDefaults.AuthenticationScheme,
        //    claimsPrincipal,
        //    authProperties );

        return true;
    }

    public async Task<LoginModel?> ValidateUserAsync( string username, string password, bool updateOldMd5 = false )
    {
        string? connectionString = _configuration.GetConnectionString("DefaultConnection");

        using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();

        // 1️⃣ Probeer PasswordHash (bcrypt)
        using var cmd = new MySqlCommand(
            "SELECT user_id, username, role, password, PasswordHash FROM ah_beheer WHERE username = @username",
            connection);
        cmd.Parameters.AddWithValue( "@username", username );

        using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow);

        if ( !await reader.ReadAsync() )
            return null;

        string? passwordHash = reader["PasswordHash"] as string;
        string? oldMd5Password = reader["password"] as string;

        bool valid = false;

        if ( !string.IsNullOrEmpty( passwordHash ) )
        {
            // Bcrypt check
            valid = BCrypt.Net.BCrypt.Verify( password, passwordHash );
        }
        else if ( !string.IsNullOrEmpty( oldMd5Password ) )
        {
            // MD5 fallback
            string hashedInput = ComputeMd5Hash(password);
            if ( hashedInput == oldMd5Password )
            {
                valid = true;

                if ( updateOldMd5 )
                {
                    // Nieuwe bcrypt hash genereren
                    string newHash = BCrypt.Net.BCrypt.HashPassword(password);

                    // Update PasswordHash kolom in DB
                    await reader.CloseAsync(); // reader moet eerst gesloten worden
                    using var updateCmd = new MySqlCommand(
                        "UPDATE ah_beheer SET PasswordHash = @hash WHERE username = @username",
                        connection);
                    updateCmd.Parameters.AddWithValue( "@hash", newHash );
                    updateCmd.Parameters.AddWithValue( "@username", username );
                    await updateCmd.ExecuteNonQueryAsync();
                }
            }
        }

        if ( !valid )
            return null;

        return new LoginModel
        {
            UserId = reader.GetInt32( "user_id" ),
            Username = reader.GetString( "username" ),
            Role = reader.GetString( "role" ).Trim().ToLowerInvariant()
        };
    }

    public static string ComputeMd5Hash( string input )
    {
        using MD5 md5 = MD5.Create();
        byte[] inputBytes = Encoding.UTF8.GetBytes(input);
        byte[] hashBytes = md5.ComputeHash(inputBytes);
        return BitConverter.ToString( hashBytes ).Replace( "-", "" ).ToLowerInvariant();
    }
}
