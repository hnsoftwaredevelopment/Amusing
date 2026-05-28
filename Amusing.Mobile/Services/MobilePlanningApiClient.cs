using System.Net.Http.Json;
using Amusing.Mobile.Shared.Models;

namespace Amusing.Mobile.Services;

public class MobilePlanningApiClient(HttpClient httpClient)
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<MobileFestivalPlanningDto?> GetCurrentPlanningAsync(CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<MobileFestivalPlanningDto>(
            "api/mobile/current-performances",
            cancellationToken);
    }
}
