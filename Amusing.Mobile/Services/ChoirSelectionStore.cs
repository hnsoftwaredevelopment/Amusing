using System.Text.Json;

namespace Amusing.Mobile.Services;

public class ChoirSelectionStore
{
    private const string PreferenceKey = "selected-choir-ids";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public IReadOnlySet<uint> ReadSelectedChoirIds()
    {
        string json = Preferences.Get(PreferenceKey, "[]");
        uint[]? ids = JsonSerializer.Deserialize<uint[]>(json, JsonOptions);
        return new HashSet<uint>(ids ?? []);
    }

    public void WriteSelectedChoirIds(IEnumerable<uint> choirIds)
    {
        uint[] sortedIds = [.. choirIds.Distinct().Order()];
        string json = JsonSerializer.Serialize(sortedIds, JsonOptions);
        Preferences.Set(PreferenceKey, json);
    }

    public void Toggle(uint choirId)
    {
        HashSet<uint> ids = [.. ReadSelectedChoirIds()];
        if (!ids.Add(choirId))
            ids.Remove(choirId);

        WriteSelectedChoirIds(ids);
    }
}
