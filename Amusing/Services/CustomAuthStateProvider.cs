using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;
using Amusing.Models;

namespace Amusing.Services;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly ISessionStorageService _sessionStorage;
    private ClaimsPrincipal _anonymous = new(new ClaimsIdentity());

    public CustomAuthStateProvider( ISessionStorageService sessionStorage )
    {
        _sessionStorage = sessionStorage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var storedUser = await _sessionStorage.GetItemAsync<LoginModel>("loggedUser");
            if ( storedUser != null )
            {
                var user = CreateClaimsPrincipal(storedUser);
                return new AuthenticationState( user );
            }
        }
        catch
        {
            // No session available or storage not readable
        }

        return new AuthenticationState( _anonymous );
    }

    public async Task MarkUserAsAuthenticated( LoginModel user )
    {
        // Store user in session storage
        await _sessionStorage.SetItemAsync( "loggedUser", user );

        // Create claims
        var authenticatedUser = CreateClaimsPrincipal(user);
        NotifyAuthenticationStateChanged( Task.FromResult( new AuthenticationState( authenticatedUser ) ) );
    }

    public async Task MarkUserAsLoggedOut()
    {
        await _sessionStorage.RemoveItemAsync( "loggedUser" );
        NotifyAuthenticationStateChanged( Task.FromResult( new AuthenticationState( _anonymous ) ) );
    }

    private ClaimsPrincipal CreateClaimsPrincipal( LoginModel user )
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.Username),
            new("UserId", user.UserId.ToString()),
            new(ClaimTypes.Role, user.Role ?? "")
        };

        var identity = new ClaimsIdentity(claims, "apiauth");
        return new ClaimsPrincipal( identity );
    }
}
