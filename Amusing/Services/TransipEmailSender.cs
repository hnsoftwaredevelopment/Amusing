using Microsoft.AspNetCore.Identity;

namespace Amusing.Services;

public class TransipEmailSender<TUser> : IEmailSender<TUser> where TUser : class
{
    private readonly TransipMailingService _mailingService;

    public TransipEmailSender( TransipMailingService mailingService )
    {
        _mailingService = mailingService;
    }

    public Task SendConfirmationLinkAsync( TUser user, string email, string confirmationLink )
    {
        return _mailingService.SendAsync( email, "Bevestig je account",
            $"Klik op de volgende link om je account te bevestigen: {confirmationLink}" );
    }

    public Task SendPasswordResetLinkAsync( TUser user, string email, string resetLink )
    {
        return _mailingService.SendAsync( email, "Reset je wachtwoord",
            $"Klik op de volgende link om je wachtwoord te resetten: {resetLink}" );
    }

    public Task SendPasswordResetCodeAsync( TUser user, string email, string resetCode )
    {
        return _mailingService.SendAsync( email, "Reset code",
            $"Gebruik deze code om je wachtwoord te resetten: {resetCode}" );
    }
}
