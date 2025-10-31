using System.Globalization;
using System.Security.Claims;

using Amusing.Helpers;
using Amusing.Models;

using BootstrapBlazor.Components;

using Microsoft.AspNetCore.Components.Authorization;

using Syncfusion.Blazor.Data;

namespace Amusing.Services;

public class LoggingService
{
    private readonly GenericDataService _dataService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly AuthenticationStateProvider _authProvider;
    private readonly UserContextHelper _userContextHelper;

    public LoggingService(
        GenericDataService dataService,
        IHttpContextAccessor httpContextAccessor,
        AuthenticationStateProvider authProvider,
        UserContextHelper userContextHelper )
    {
        _dataService = dataService;
        _httpContextAccessor = httpContextAccessor;
        _authProvider = authProvider;
        _userContextHelper = userContextHelper;
    }

    #region Full logs
    public Task<List<LogModel>> GetUsersLogAsync()
    {
        return _dataService.ExecuteQueryAsync( SPDefinitions.RunGetUsersLog, reader =>
        {
            return new LogModel
            {
                LogDate = reader [ "date" ] != DBNull.Value ? Convert.ToDateTime( reader [ "date" ] ) : null,
                LogArea = reader [ "area" ].ToString(),
                LogAction = reader [ "action" ].ToString(),
                LogReport = reader [ "report" ].ToString()
            };
        } );
    }

    public Task<List<LogModel>> GetPersonsLogAsync()
    {
        return _dataService.ExecuteQueryAsync( SPDefinitions.RunGetPersonsLog, reader =>
        {
            return new LogModel
            {
                LogDate = reader [ "date" ] != DBNull.Value ? Convert.ToDateTime( reader [ "date" ] ) : null,
                LogArea = reader [ "area" ].ToString(),
                LogAction = reader [ "action" ].ToString(),
                LogReport = reader [ "report" ].ToString()
            };
        } );
    }
    #endregion

    #region Login logs
    public Task<List<LogModel>> GetUserLoginsAsync()
    {
        return _dataService.ExecuteQueryAsync( SPDefinitions.RunGetUserLoginsLog, reader =>
        {
            return new LogModel
            {
                LogDate = reader [ "date" ] != DBNull.Value ? Convert.ToDateTime( reader [ "date" ] ) : null,
                LogArea = reader [ "area" ].ToString(),
                LogAction = reader [ "action" ].ToString(),
                LogReport = reader [ "report" ].ToString()
            };
        } );
    }

    public async Task WriteUserLoginAttemptAsync( string _status, string _report )
    {
        if ( _report == "" )
        {
            return;
        }

        int _userId = _userContextHelper.GetUserId();
        string _userIp = _userContextHelper.GetUserIp();

        Dictionary<string, object> parameters = new()
        {
            { "@UserId", _userId },
            { "@UserIp", _userIp },
            { "@Area", "Toegang" },
            { "@Action", "Inloggen" },
            { "@Status", _status },
            { "@Report", _report }
        };

        try
        {
            await _dataService.ExecuteNonQueryAsync( QueryDefinitions.LogUserLogin, parameters );
        }
        catch ( Exception ex )
        {
            Dictionary<string, object> err_parameters = new()
            {
                { "@UserIp", _userIp },
                { "@Area", "Toegang" },
                { "@Action", "Inloggen" },
                { "@Status", "Fout" },
                { "@Report", ex.Message }
            };

            await _dataService.ExecuteNonQueryAsync( QueryDefinitions.LogError, err_parameters );
        }
        return;
    }

    public async Task WriteUserLogoutAttemptAsync( string _status, string _report )
    {
        if ( _report == "" )
        {
            return;
        }

        var authState = await _authProvider.GetAuthenticationStateAsync();
        var claimUser = authState.User;
        string? _userId = claimUser.FindFirst("UserId")?.Value;
        
        string _userIp = _userContextHelper.GetUserIp();

        Dictionary<string, object> parameters = new()
        {
            { "@UserId", _userId },
            { "@UserIp", _userIp },
            { "@Area", "Toegang" },
            { "@Action", "Uitloggen" },
            { "@Status", _status },
            { "@Report", _report }
        };

        try
        {
            await _dataService.ExecuteNonQueryAsync( QueryDefinitions.LogUserLogin, parameters );
        }
        catch ( Exception ex )
        {
            Dictionary<string, object> err_parameters = new()
            {
                { "@UserIp", _userIp },
                { "@Area", "Toegang" },
                { "@Action", "Uitloggen" },
                { "@Status", "Error" },
                { "@Report", ex.Message }
            };

            await _dataService.ExecuteNonQueryAsync( QueryDefinitions.LogError, err_parameters );
        }
        return;
    }

    public async Task WriteUserActionAsync( string _area, string _action, string _status, string _report )
    {
        if ( _report == "" )
        {
            return;
        }

        var authState = await _authProvider.GetAuthenticationStateAsync();
        var claimUser = authState.User;
        string? _userId = claimUser.FindFirst("UserId")?.Value;
        string? userName = ((System.Security.Claims.ClaimsIdentity?)claimUser.Identity)?.Name;
        userName = string.IsNullOrWhiteSpace( userName ) ? "Een onbekende gebruiker" : userName;
        _report = ( _report ?? string.Empty ).Replace( "<_userName>", userName );


        string _userIp = _userContextHelper.GetUserIp();

        Dictionary<string, object> parameters = new()
        {
            { "@UserId", _userId },
            { "@UserIp", _userIp },
            { "@Area", _area },
            { "@Action", _action },
            { "@Status", _status },
            { "@Report", _report }
        };

        try
        {
            await _dataService.ExecuteNonQueryAsync( QueryDefinitions.LogUserLogin, parameters );
        }
        catch ( Exception ex )
        {
            Dictionary<string, object> err_parameters = new()
            {
                { "@UserIp", _userIp },
                { "@Area", _area },
                { "@Action", _action },
                { "@Status", "Error" },
                { "@Report", ex.Message }
            };

            await _dataService.ExecuteNonQueryAsync( QueryDefinitions.LogError, err_parameters );
        }
        return;
    }

    public async Task WriteUserActionGroupAsync( uint _groupId, string _area, string _action, string _status, string _report )
    {
        if ( _report == "" )
        {
            return;
        }

        var authState = await _authProvider.GetAuthenticationStateAsync();
        var claimUser = authState.User;
        string? _userId = claimUser.FindFirst("UserId")?.Value;
        string? userName = ((System.Security.Claims.ClaimsIdentity?)claimUser.Identity)?.Name;
        userName = string.IsNullOrWhiteSpace( userName ) ? "Een onbekende gebruiker" : userName;
        _report = ( _report ?? string.Empty ).Replace( "<_userName>", userName );


        string _userIp = _userContextHelper.GetUserIp();

        Dictionary<string, object> parameters = new()
        {
            { "@UserId", _userId },
            { "@UserIp", _userIp },
            { "@GroupId", _groupId },
            { "@Area", _area },
            { "@Action", _action },
            { "@Status", _status },
            { "@Report", _report }
        };

        try
        {
            await _dataService.ExecuteNonQueryAsync( QueryDefinitions.LogGroupActions, parameters );
        }
        catch ( Exception ex )
        {
            Dictionary<string, object> err_parameters = new()
            {
                { "@UserIp", _userIp },
                { "@Area", _area },
                { "@Action", _action },
                { "@Status", "Error" },
                { "@Report", ex.Message }
            };

            await _dataService.ExecuteNonQueryAsync( QueryDefinitions.LogError, err_parameters );
        }
        return;
    }

    public async Task WriteUserActionFestivalAsync( uint _festivalId, string _area, string _action, string _status, string _report )
    {
        if ( _report == "" )
        {
            return;
        }

        var authState = await _authProvider.GetAuthenticationStateAsync();
        var claimUser = authState.User;
        string? _userId = claimUser.FindFirst("UserId")?.Value;
        string? userName = ((System.Security.Claims.ClaimsIdentity?)claimUser.Identity)?.Name;
        userName = string.IsNullOrWhiteSpace( userName ) ? "Een onbekende gebruiker" : userName;
        _report = ( _report ?? string.Empty ).Replace( "<_userName>", userName );


        string _userIp = _userContextHelper.GetUserIp();

        Dictionary<string, object> parameters = new()
        {
            { "@UserId", _userId },
            { "@UserIp", _userIp },
            { "@FestivalId", _festivalId },
            { "@Area", _area },
            { "@Action", _action },
            { "@Status", _status },
            { "@Report", _report }
        };

        try
        {
            await _dataService.ExecuteNonQueryAsync( QueryDefinitions.LogFestivalActions, parameters );
        }
        catch ( Exception ex )
        {
            Dictionary<string, object> err_parameters = new()
            {
                { "@UserIp", _userIp },
                { "@Area", _area },
                { "@Action", _action },
                { "@Status", "Error" },
                { "@Report", ex.Message }
            };

            await _dataService.ExecuteNonQueryAsync( QueryDefinitions.LogError, err_parameters );
        }
        return;
    }

    public async Task WriteUserActionStageAsync( uint _stageId, string _area, string _action, string _status, string _report )
    {
        if ( _report == "" )
        {
            return;
        }

        var authState = await _authProvider.GetAuthenticationStateAsync();
        var claimUser = authState.User;
        string? _userId = claimUser.FindFirst("UserId")?.Value;
        string? userName = ((System.Security.Claims.ClaimsIdentity?)claimUser.Identity)?.Name;
        userName = string.IsNullOrWhiteSpace( userName ) ? "Een onbekende gebruiker" : userName;
        _report = ( _report ?? string.Empty ).Replace( "<_userName>", userName );


        string _userIp = _userContextHelper.GetUserIp();

        Dictionary<string, object> parameters = new()
        {
            { "@UserId", _userId },
            { "@UserIp", _userIp },
            { "@StageId", _stageId },
            { "@Area", _area },
            { "@Action", _action },
            { "@Status", _status },
            { "@Report", _report }
        };

        try
        {
            await _dataService.ExecuteNonQueryAsync( QueryDefinitions.LogStageActions, parameters );
        }
        catch ( Exception ex )
        {
            Dictionary<string, object> err_parameters = new()
            {
                { "@UserIp", _userIp },
                { "@Area", _area },
                { "@Action", _action },
                { "@Status", "Error" },
                { "@Report", ex.Message }
            };

            await _dataService.ExecuteNonQueryAsync( QueryDefinitions.LogError, err_parameters );
        }
        return;
    }

    public Task<List<LogModel>> GetPersonLoginsAsync()
    {
        return _dataService.ExecuteQueryAsync( SPDefinitions.RunGetPersonLoginsLog, reader =>
        {
            return new LogModel
            {
                LogDate = reader [ "date" ] != DBNull.Value ? Convert.ToDateTime( reader [ "date" ] ) : null,
                LogArea = reader [ "area" ].ToString(),
                LogAction = reader [ "action" ].ToString(),
                LogReport = reader [ "report" ].ToString()
            };
        } );
    }

    #endregion

    #region Payment logs
    public Task<List<LogModel>> GetPaymentsLogAsync()
    {
        return _dataService.ExecuteQueryAsync( SPDefinitions.RunGetPaymentsLog, reader =>
        {
            return new LogModel
            {
                LogDate = reader [ "date" ] != DBNull.Value ? Convert.ToDateTime( reader [ "date" ] ) : null,
                LogArea = reader [ "area" ].ToString(),
                LogAction = reader [ "action" ].ToString(),
                LogReport = reader [ "report" ].ToString()
            };
        } );
    }
    #endregion

    private ClaimsPrincipal CreateClaimsPrincipal( LoginModel user )
    {
        var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.Username),
        new Claim(ClaimTypes.Role, user.Role ?? ""),
        new Claim("UserId", user.UserId.ToString())
    };

        var identity = new ClaimsIdentity(claims, "apiauth");
        return new ClaimsPrincipal( identity );
    }
}
