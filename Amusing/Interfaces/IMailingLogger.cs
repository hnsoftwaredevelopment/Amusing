namespace Amusing.Interfaces;

public interface IMailingLogger
{
    Task LogMailSentAsync( string recipient, string subject, bool success, string? errorMessage = null );
    Task LogPreviewGeneratedAsync( string recipient, string subject );
}
