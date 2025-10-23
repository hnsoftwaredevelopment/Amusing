using Microsoft.Extensions.Logging;

using System;
using System.Threading.Tasks;

namespace Amusing.Services;

public interface IMailingLogger
{
    Task LogMailSentAsync( string recipient, string subject, bool success, string? errorMessage = null );
    void LogWarning( string message );
    void LogError( string message, Exception? ex = null );
}

public class MailingLogger : IMailingLogger
{
    private readonly ILogger<MailingLogger> _logger;

    public MailingLogger( ILogger<MailingLogger> logger )
    {
        _logger = logger;
    }

    public Task LogMailSentAsync( string recipient, string subject, bool success, string? errorMessage = null )
    {
        if ( success )
        {
            _logger.LogInformation( "Mail sent to {Recipient} with subject '{Subject}'", recipient, subject );
        }
        else
        {
            _logger.LogError( "Mail delivery failed to {Recipient} for subject '{Subject}'. Error: {Error}",
                recipient, subject, errorMessage ?? "Unknown error" );
        }

        return Task.CompletedTask;
    }

    public void LogWarning( string message )
        => _logger.LogWarning( message );

    public void LogError( string message, Exception? ex = null )
    {
        if ( ex == null )
            _logger.LogError( message );
        else
            _logger.LogError( ex, message );
    }
}
