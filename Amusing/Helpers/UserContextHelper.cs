using System.Security.Claims;

using Microsoft.AspNetCore.Components.Authorization;

namespace Amusing.Helpers;

public class UserContextHelper
{
    private readonly AuthenticationStateProvider _authProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;

    private int? _cachedUserId;
    private string? _cachedUsername;

    public UserContextHelper( AuthenticationStateProvider authProvider, IHttpContextAccessor httpContextAccessor )
    {
        _authProvider = authProvider;
        _httpContextAccessor = httpContextAccessor;
    }

    public void SetUserContext( int userId, string username )
    {
        _cachedUserId = userId;
        _cachedUsername = username;
    }

    public int GetUserId()
    {
        if ( _cachedUserId.HasValue )
            return _cachedUserId.Value;

        var claim = (_authProvider.GetAuthenticationStateAsync().Result).User.FindFirst(ClaimTypes.NameIdentifier);
        return int.TryParse( claim?.Value, out var id ) ? id : 0;
    }

    public string GetUsername()
    {
        if ( !string.IsNullOrEmpty( _cachedUsername ) )
            return _cachedUsername;

        return ( _authProvider.GetAuthenticationStateAsync().Result ).User.Identity?.Name ?? "Onbekend";
    }

    public string GetUserIp()
    {
        var ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

        // Handle localhost IPv6 (::1)
        if ( string.IsNullOrEmpty( ip ) || ip == "::1" )
            ip = "127.0.0.1 (localhost)";

        // Optional: check for reverse proxy forwarding
        if ( _httpContextAccessor.HttpContext?.Request.Headers.TryGetValue( "X-Forwarded-For", out var forwarded ) == true )
            ip = forwarded.FirstOrDefault() ?? ip;

        return ip ?? "Onbekend";
    }
}
