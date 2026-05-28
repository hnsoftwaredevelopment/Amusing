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

        try
        {
            await using FileStream stream = File.OpenRead(_cachePath);
            return await JsonSerializer.DeserializeAsync<MobileFestivalPlanningDto>(stream, JsonOptions, cancellationToken);
        }
        catch (JsonException)
        {
            return null;
        }
        catch (IOException)
        {
            return null;
        }
    }

    public async Task WriteAsync(MobileFestivalPlanningDto planning, CancellationToken cancellationToken = default)
    {
        string tempPath = Path.Combine(FileSystem.AppDataDirectory, $"{Path.GetFileName(_cachePath)}.tmp");

        await using (FileStream stream = File.Create(tempPath))
        {
            await JsonSerializer.SerializeAsync(stream, planning, JsonOptions, cancellationToken);
        }

        File.Move(tempPath, _cachePath, overwrite: true);
    }
}
