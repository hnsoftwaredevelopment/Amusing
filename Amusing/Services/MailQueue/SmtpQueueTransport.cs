using Amusing.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Amusing.Services.MailQueue;

public sealed class SmtpQueueTransport(EmailSettings settings) : IQueueMailTransport
{
    public async Task<MailAttempt> SendAsync(byte[] mime, CancellationToken cancellationToken)
    {
        bool sending = false;
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromMinutes(2));
        using var client = new SmtpClient();
        try
        {
            using var data = new MemoryStream(mime, writable: false);
            var message = await MimeMessage.LoadAsync(data, timeout.Token);
            await client.ConnectAsync(settings.SmtpHost, settings.SmtpPort,
                settings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None, timeout.Token);
            if (!string.IsNullOrWhiteSpace(settings.SmtpUser))
                await client.AuthenticateAsync(settings.SmtpUser, settings.SmtpPass, timeout.Token);
            sending = true;
            await client.SendAsync(message, timeout.Token);
            // SMTP has accepted responsibility. QUIT/logging failures must never trigger another Send.
            try { await client.DisconnectAsync(true, timeout.Token); } catch { /* Accepted already. */ }
            return new(MailOutcome.Accepted);
        }
        catch (MailKit.Security.AuthenticationException) { return new(MailOutcome.ConfigurationFailure, "SMTP-aanmelding mislukt."); }
        catch (SmtpCommandException ex)
        {
            int code = (int)ex.StatusCode;
            if (code is 530 or 534 or 535 or 538) return new(MailOutcome.ConfigurationFailure, ex.Message, code);
            if (code is >= 400 and < 500)
                return new(MailOutcome.TemporaryFailure, ex.Message, code,
                    ex.Message.Contains("quota", StringComparison.OrdinalIgnoreCase));
            return new(code is >= 500 and < 600 ? MailOutcome.PermanentFailure : MailOutcome.Uncertain, ex.Message, code);
        }
        catch (SslHandshakeException) { return new(MailOutcome.ConfigurationFailure, "SMTP TLS-verbinding mislukt."); }
        catch (Exception ex) when (ex is IOException or System.Net.Sockets.SocketException or OperationCanceledException or MailKit.ProtocolException)
        {
            return new(sending ? MailOutcome.Uncertain : MailOutcome.TemporaryFailure,
                sending ? "SMTP-uitkomst onbekend; controleer voor opnieuw verzenden." : "SMTP tijdelijk niet bereikbaar.");
        }
        catch (Exception) { return new(MailOutcome.ConfigurationFailure, "SMTP-configuratie of bericht kon niet worden verwerkt."); }
    }
}
