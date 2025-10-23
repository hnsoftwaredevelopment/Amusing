using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

using Amusing.Models;

using Microsoft.Extensions.Logging;

namespace Amusing.Services;

// Handles email sending through the TransIP Mail API
public class TransipMailingService
{
    private readonly HttpClient _httpClient;
    private readonly EmailSettings _settings;
    private readonly ILogger<TransipMailingService> _logger;

    public TransipMailingService( HttpClient httpClient, EmailSettings settings, ILogger<TransipMailingService> logger )
    {
        _settings = settings;
        _logger = logger;

        Debug.WriteLine( "TransipMailingService:" );
        Debug.WriteLine( $"SMTP: {settings.SmtpHost}:{settings.SmtpPort}" );
        Debug.WriteLine( $"User: {settings.SmtpUser}   PW: {settings.SmtpPass}" );

        // Log errors if configuration seems incomplete
        if ( string.IsNullOrWhiteSpace( _settings.SmtpHost ) )
            _logger.LogError( "SMTP host is missing in EmailSettings.SmtpHost." );

        if ( string.IsNullOrWhiteSpace( _settings.SmtpUser ) || string.IsNullOrWhiteSpace( _settings.SmtpPass ) )
            _logger.LogError( "SMTP credentials are missing or incomplete." );
    }

    public async Task SendAsync( string to, string subject, string body )
    {

        Debug.WriteLine( "SendAsync:" );
        Debug.WriteLine( $"SMTP: {_settings.SmtpHost}:{_settings.SmtpPort}" );
        Debug.WriteLine( $"User: {_settings.SmtpUser}" );
        if ( string.IsNullOrWhiteSpace( to ) )

        {
            _logger.LogWarning( "Email not sent: recipient address is empty." );
            return;
        }

        var payload = new
        {
            to = new[] { to },
            from = _settings.SenderAddress ?? "noreply@amusing-hengelo.nl",
            subject,
            html = body
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            _logger.LogInformation( "Sending mail to {Recipient} via {BaseUrl}", to, _httpClient.BaseAddress );
            var response = await _httpClient.PostAsync("send", content);

            if ( !response.IsSuccessStatusCode )
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError( "Mail send failed: {Status} - {Error}", response.StatusCode, error );
            }
            else
            {
                _logger.LogInformation( "Mail successfully sent to {Recipient}", to );
            }
        }
        catch ( Exception ex )
        {
            _logger.LogError( ex, "Error while sending email to {Recipient}", to );
            throw;
        }
    }
}
