using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Amusing.Services;

// Handles identity-related emails using TransIP Mail API
public class TransipEmailSender<TUser> : IEmailSender<TUser> where TUser : class
{
    private readonly TransipMailingService _mailingService;
    private readonly ILogger<TransipEmailSender<TUser>> _logger;

    public TransipEmailSender( TransipMailingService mailingService, ILogger<TransipEmailSender<TUser>> logger )
    {
        _mailingService = mailingService;
        _logger = logger;
    }

    public async Task SendConfirmationLinkAsync( TUser user, string email, string confirmationLink )
    {
        try
        {
            _logger.LogInformation( "Sending confirmation link to {Email}", email );
            await _mailingService.SendAsync(
                email,
                "Bevestig je account",
                $"Klik op de volgende link om je account te bevestigen:<br/><br/><a href='{confirmationLink}'>{confirmationLink}</a>"
            );
        }
        catch ( Exception ex )
        {
            _logger.LogError( ex, "Error sending confirmation link to {Email}", email );
            throw;
        }
    }

    public async Task SendPasswordResetLinkAsync( TUser user, string email, string resetLink )
    {
        try
        {
            _logger.LogInformation( "Sending password reset link to {Email}", email );
            await _mailingService.SendAsync(
                email,
                "Reset je wachtwoord",
                $"Klik op de volgende link om je wachtwoord te resetten:<br/><br/><a href='{resetLink}'>{resetLink}</a>"
            );
        }
        catch ( Exception ex )
        {
            _logger.LogError( ex, "Error sending password reset link to {Email}", email );
            throw;
        }
    }

    public async Task SendPasswordResetCodeAsync( TUser user, string email, string resetCode )
    {
        try
        {
            _logger.LogInformation( "Sending password reset code to {Email}", email );
            await _mailingService.SendAsync(
                email,
                "Reset code",
                $"Gebruik deze code om je wachtwoord te resetten: <b>{resetCode}</b>"
            );
        }
        catch ( Exception ex )
        {
            _logger.LogError( ex, "Error sending password reset code to {Email}", email );
            throw;
        }
    }
}
