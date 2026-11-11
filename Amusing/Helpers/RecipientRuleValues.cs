using System.Collections;
using System.Globalization;
using System.Text.Json;

namespace Amusing.Helpers;

public static class RecipientRuleValues
{
    public static string[] ToSelection(object? value) => value switch
    {
        null => [],
        string text => [text],
        JsonElement { ValueKind: JsonValueKind.Null or JsonValueKind.Undefined } => [],
        JsonElement { ValueKind: JsonValueKind.Array } json => json.EnumerateArray()
            .SelectMany(item => ToSelection(item)).ToArray(),
        JsonElement json => [json.ToString()],
        Newtonsoft.Json.Linq.JValue scalar => ToSelection(scalar.Value),
        IEnumerable values => values.Cast<object?>().SelectMany(ToSelection).ToArray(),
        _ => [Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty]
    };
}
