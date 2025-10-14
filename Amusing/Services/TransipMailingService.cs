using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Amusing.Services;

public class TransipMailingService
{
    private readonly HttpClient _httpClient;

    public TransipMailingService( HttpClient httpClient )
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri( "https://api.transip.nl/v6/mail/" );
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue( "Bearer", "JOUW_API_TOKEN" );
    }

    public async Task SendAsync( string to, string subject, string body )
    {
        var payload = new
        {
            to = new[] { to },
            from = "noreply@jouwdomein.nl",
            subject,
            html = body
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("send", content);

        response.EnsureSuccessStatusCode();
    }
}
