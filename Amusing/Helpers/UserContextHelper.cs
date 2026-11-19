using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace Amusing.Helpers;

public class UserContextHelper(AuthenticationStateProvider authProvider, IHttpContextAccessor httpContextAccessor)
{
    public int GetUserId() => ReadUserId(httpContextAccessor.HttpContext?.User);

    public async Task<int> GetUserIdAsync()
    {
        var state = await authProvider.GetAuthenticationStateAsync();
        return ReadUserId(state.User);
    }

    public string GetUsername() => httpContextAccessor.HttpContext?.User.Identity?.Name ?? "Onbekend";

    public async Task<string> GetUsernameAsync()
    {
        var state = await authProvider.GetAuthenticationStateAsync();
        return state.User.Identity?.Name ?? "Onbekend";
    }

    public string GetUserIp() => ClientIpAddressHelper.GetClientIp(httpContextAccessor.HttpContext);

    private static int ReadUserId(ClaimsPrincipal? user)
    {
        var claim = user?.FindFirst("UserId") ?? user?.FindFirst(ClaimTypes.NameIdentifier);
        return user?.Identity?.IsAuthenticated == true && int.TryParse(claim?.Value, out var id) ? id : 0;
    }
}
