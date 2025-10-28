using System.Globalization;
using System.Security.Claims;

using Amusing.Helpers;
using Amusing.Models;

using BootstrapBlazor.Components;

using Microsoft.AspNetCore.Components.Authorization;

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

    public async Task WritUserLoginAttemptAsync( string _status, string _report )
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

}
