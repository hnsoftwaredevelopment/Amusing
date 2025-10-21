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
        _httpClient = httpClient;
        _settings = settings;
        _logger = logger;

        // Configure HttpClient base settings
        _httpClient.BaseAddress = new Uri( _settings.SmtpHost ?? "https://api.transip.nl/v6/mail/" );

        // If the API token is missing, log a clear error
        if ( string.IsNullOrWhiteSpace( _settings.SmtpPass ) )
        {
            _logger.LogError( "TransIP API token is missing in EmailSettings.Password." );
        }
        else
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue( "Bearer", _settings.SmtpPass );
        }
    }

    public async Task SendAsync( string to, string subject, string body )
    {
        if ( string.IsNullOrWhiteSpace( to ) )
        {
            _logger.LogWarning( "Email not sent: recipient address is empty." );
            return;
        }

        var payload = new
        {
            to = new[] { to },
            from = _settings.SenderAddress ?? "noreply@jouwdomein.nl",
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
