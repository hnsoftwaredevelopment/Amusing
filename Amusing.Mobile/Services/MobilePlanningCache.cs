using System.Text.Json;
using Amusing.Mobile.Shared.Models;

namespace Amusing.Mobile.Services;

public class MobilePlanningCache
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly string _cachePath = Path.Combine(FileSystem.AppDataDirectory, "mobile-planning-cache.json");

    public async Task<MobileFestivalPlanningDto?> ReadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_cachePath))
            return null;

        await using FileStream stream = File.OpenRead(_cachePath);
        return await JsonSerializer.DeserializeAsync<MobileFestivalPlanningDto>(stream, JsonOptions, cancellationToken);
    }

    public async Task WriteAsync(MobileFestivalPlanningDto planning, CancellationToken cancellationToken = default)
    {
        await using FileStream stream = File.Create(_cachePath);
        await JsonSerializer.SerializeAsync(stream, planning, JsonOptions, cancellationToken);
    }
}
