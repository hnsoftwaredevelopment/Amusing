using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Amusing.Models;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly ProtectedSessionStorage _sessionStorage;
    private ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());

    public CustomAuthStateProvider( ProtectedSessionStorage sessionStorage )
    {
        _sessionStorage = sessionStorage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var storedUser = await _sessionStorage.GetAsync<LoginModel>("loggedUser");

            if ( storedUser.Success && storedUser.Value != null )
            {
                var user = CreateClaimsPrincipal(storedUser.Value);
                return new AuthenticationState( user );
            }
        }
        catch
        {
            // Session niet leesbaar of nog niet aangemaakt
        }

        return new AuthenticationState( _anonymous );
    }

    public async Task MarkUserAsAuthenticated( LoginModel loginUser )
    {
        await _sessionStorage.SetAsync( "loggedUser", loginUser );

        var user = CreateClaimsPrincipal(loginUser);
        NotifyAuthenticationStateChanged( Task.FromResult( new AuthenticationState( user ) ) );
    }

    public async Task MarkUserAsLoggedOut()
    {
        await _sessionStorage.DeleteAsync( "loggedUser" );
        NotifyAuthenticationStateChanged( Task.FromResult( new AuthenticationState( _anonymous ) ) );
    }

    private ClaimsPrincipal CreateClaimsPrincipal( LoginModel user )
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var identity = new ClaimsIdentity(claims, "CustomAuth");
        return new ClaimsPrincipal( identity );
    }
}
