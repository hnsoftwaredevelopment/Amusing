using System.Security.Claims;

using Blazored.SessionStorage;

using Microsoft.AspNetCore.Components.Authorization;

namespace Amusing.Helpers;

public class UserContextHelper
{
    private readonly AuthenticationStateProvider _authProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ISessionStorageService _sessionStorage;

    private int? _cachedUserId;
    private string? _cachedUsername;

    public UserContextHelper( 
        AuthenticationStateProvider authProvider, 
        IHttpContextAccessor httpContextAccessor,
        ISessionStorageService sessionStorage )
    {
        _authProvider = authProvider;
        _httpContextAccessor = httpContextAccessor;
        _sessionStorage = sessionStorage;
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

        var user = _httpContextAccessor.HttpContext?.User;
        var claim = user?.FindFirst("UserId") ?? user?.FindFirst(ClaimTypes.NameIdentifier);
        return int.TryParse( claim?.Value, out var id ) ? id : 0;
    }

    public async Task<int> GetUserIdAsync()
    {
        if ( _cachedUserId.HasValue )
            return _cachedUserId.Value;

        // Try SessionStorage
        var storedId = await _sessionStorage.GetItemAsync<int>("UserId");
        if ( storedId > 0 )
            return storedId;

        // Try ClaimsPrincipal
        var authState = await _authProvider.GetAuthenticationStateAsync();
        var claim = authState.User.FindFirst("UserId") ?? authState.User.FindFirst(ClaimTypes.NameIdentifier);
        return int.TryParse( claim?.Value, out var id ) ? id : 0;
    }

    public string GetUsername()
    {
        if ( !string.IsNullOrEmpty( _cachedUsername ) )
            return _cachedUsername;

        return _httpContextAccessor.HttpContext?.User.Identity?.Name ?? "Onbekend";
    }

    public async Task<string> GetUsernameAsync()
    {
        if ( !string.IsNullOrEmpty( _cachedUsername ) )
            return _cachedUsername;

        var storedName = await _sessionStorage.GetItemAsync<string>("Username");
        if ( !string.IsNullOrEmpty( storedName ) )
            return storedName;

        var authState = await _authProvider.GetAuthenticationStateAsync();
        return authState.User.Identity?.Name ?? "Onbekend";
    }

    public string GetUserIp()
    {
        return ClientIpAddressHelper.GetClientIp( _httpContextAccessor.HttpContext );
    }
}
