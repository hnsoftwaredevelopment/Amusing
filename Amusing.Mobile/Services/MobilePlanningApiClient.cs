using System.Net;
using System.Net.Http.Json;
using Amusing.Mobile.Shared.Models;

namespace Amusing.Mobile.Services;

public class MobilePlanningApiClient(HttpClient httpClient)
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<MobileFestivalPlanningDto?> GetCurrentPlanningAsync(CancellationToken cancellationToken = default)
    {
        using HttpResponseMessage response = await _httpClient.GetAsync(
            "api/mobile/current-performances",
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<MobileFestivalPlanningDto>(cancellationToken);
    }
}
