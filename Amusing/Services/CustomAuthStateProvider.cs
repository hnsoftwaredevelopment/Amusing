using System.Security.Claims;

using Microsoft.AspNetCore.Components.Authorization;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CustomAuthStateProvider( IHttpContextAccessor httpContextAccessor )
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        ClaimsPrincipal user = _httpContextAccessor.HttpContext?.User ?? new ClaimsPrincipal(new ClaimsIdentity());

        // ✅ Controleer of er een geldige identiteit is én deze geauthenticeerd is
        if ( user.Identity != null && user.Identity.IsAuthenticated )
        {
            return Task.FromResult( new AuthenticationState( user ) );
        }

        // ❌ Fallback naar een niet-geauthenticeerde identiteit
        ClaimsPrincipal anonymous = new(new ClaimsIdentity());
        return Task.FromResult( new AuthenticationState( anonymous ) );
    }
}