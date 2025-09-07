using System.Text.Json;

using Syncfusion.Blazor.QueryBuilder;

public static class QueryBuilderJsonConverter
{
    public static string OldToNew(string oldJson)
    {
        if (string.IsNullOrWhiteSpace(oldJson) || oldJson == "[]")
            return JsonSerializer.Serialize(new List<RuleModel>(), new JsonSerializerOptions { WriteIndented = true });

        List<OldQueryRule> oldList;
        try
        {
            oldList = JsonSerializer.Deserialize<List<OldQueryRule>>(oldJson);
        }
        catch
        {
            return JsonSerializer.Serialize(new List<RuleModel>(), new JsonSerializerOptions { WriteIndented = true });
        }

        var newRules = new List<RuleModel>();

        foreach (var old in oldList)
        {
            var fieldName = old.field?.value ?? string.Empty;
            var op = MapOperatorToNew(old.@operator?.value ?? string.Empty);
            var parsedValue = ParseValue(old);

            // --- Fix old contactpersonen ---
            parsedValue = FixOldContactPersons(parsedValue);

            // --- Special cases ---
            if (fieldName.Equals("enrolled", StringComparison.OrdinalIgnoreCase))
            {
                fieldName = "Festival";
                op = "in"; // altijd multi-select
                parsedValue = EnsureArray(parsedValue);
            }
            else if (fieldName.Equals("role", StringComparison.OrdinalIgnoreCase) ||
                     fieldName.Equals("volunteered", StringComparison.OrdinalIgnoreCase))
            {
                parsedValue = EnsureArray(parsedValue);
                op = "in";
            }

            // --- Map field names ---
            string newField = fieldName switch
            {
                "role" => "Role",
                "volunteered" => "Volunteer",
                "mailing" => "Infomailing",
                "droppedout" => "IsCanceled",
                "payed" => "IsPaid",
                "review" => "Jury",
                "headcount" => "Singers",
                "dressingroom" => "Dressingroom",
                "enrolled" => "Festival",
                _ => fieldName
            };

            // --- Determine type ---
            string type = DetermineType(parsedValue, op);

            newRules.Add(new RuleModel
            {
                Field = newField,
                Label = old.field?.label ?? string.Empty,
                Operator = op,
                Type = type,
                Value = parsedValue,
                Rules = null
            });
        }

        return JsonSerializer.Serialize(newRules, new JsonSerializerOptions { WriteIndented = true });
    }

    public static string NewToOld(SfQueryBuilder<RuleModel> queryBuilder)
    {
        // Haal de huidige regels uit de querybuilder
        var currentRules = queryBuilder.GetRules();

        if (currentRules == null)
            return string.Empty;

        // Zet om naar jouw oude JSON formaat
        // Voorbeeld: alles naar een "flat list" van RuleModels
        var rulesList = currentRules.Rules ?? new List<RuleModel>();

        // Serialize als oude JSON structuur
        return JsonSerializer.Serialize(rulesList, new JsonSerializerOptions
        {
            WriteIndented = false
        });
    }

    private static object EnsureArray(object value)
    {
        if (value == null) return Array.Empty<string>();

        if (value is string s)
            return s.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .ToArray();

        if (value is int i)
            return new[] { i.ToString() };

        if (value is string[] arr)
            return arr;

        if (value is IEnumerable<string> strEnum)
            return strEnum.ToArray();

        return new[] { value.ToString() };
    }
    private static string DetermineType(object value, string op)
    {
        if (op == "in") return "String";
        if (value is bool) return "Boolean";
        if (value is int) return "Number";
        return "String";
    }

    private static object ParseValue(OldQueryRule rule)
    {
        if (rule.value == null) return null;

        var fieldName = rule.field?.value ?? string.Empty;

        // --- Known boolean fields ---
        if (BooleanFields.Contains(fieldName, StringComparer.OrdinalIgnoreCase))
        {
            if (rule.value is JsonElement je)
            {
                if (je.ValueKind == JsonValueKind.Number)
                    return je.GetInt32() != 0;
                if (je.ValueKind == JsonValueKind.True) return true;
                if (je.ValueKind == JsonValueKind.False) return false;
                if (je.ValueKind == JsonValueKind.String)
                {
                    var str = je.GetString();
                    if (str == "0") return false;
                    if (str == "1") return true;
                    if (bool.TryParse(str, out bool b)) return b;
                }
            }
            else if (rule.value is int i)
            {
                return i != 0;
            }
            else if (rule.value is string s)
            {
                if (s == "0") return false;
                if (s == "1") return true;
                if (bool.TryParse(s, out bool b)) return b;
            }
        }

        // --- Original JsonElement handling ---
        if (rule.value is JsonElement je2)
        {
            if (je2.ValueKind == JsonValueKind.Array)
                return je2.EnumerateArray().Select(x => x.ToString()).ToArray();
            if (je2.ValueKind == JsonValueKind.Number)
                return je2.GetInt32();
            if (je2.ValueKind == JsonValueKind.String)
                return je2.GetString();
            if (je2.ValueKind == JsonValueKind.True) return true;
            if (je2.ValueKind == JsonValueKind.False) return false;
        }

        return rule.value;
    }
    private static string MapOperatorToNew(string oldOp)
    {
        return oldOp switch
        {
            "eq" => "equal",
            "in" => "in",
            "gt" => "greaterthan",
            "lt" => "lessthan",
            _ => oldOp
        };
    }

    private static object FixOldContactPersons(object value)
    {
        if (value is string s)
        {
            if (s.Contains("contactpersoon")) s = s.Replace("contactpersoon", "contactpersoon1");
            if (s.Equals("contact", StringComparison.OrdinalIgnoreCase)) s = "contactpersoon1";
            if (s.Contains("contact2")) s = s.Replace("contact2", "contactpersoon2");
            return s.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();
        }
        if (value is string[] arr)
        {
            return arr.Select(x =>
            {
                if (x == "contactpersoon") return "contactpersoon1";
                if (x == "contact2") return "contactpersoon2";
                if (x.Equals("contact", StringComparison.OrdinalIgnoreCase)) return "contactpersoon1";
                return x;
            }).ToArray();
        }
        return value;
    }

    private static readonly HashSet<string> BooleanFields =
[
    "IsCanceled",
    "IsPaid",
    "Dressingroom",
    "Jury",
    "Infomailing"
];

    #region OldQueryRule
    public class OldQueryRule
    {
        public OldField field { get; set; }
        public OldOperator @operator { get; set; }
        public object value { get; set; }
    }

    public class OldField
    {
        public string value { get; set; }
        public string label { get; set; }
    }

    public class OldOperator
    {
        public string value { get; set; }
    }
    #endregion

    #region LoadRulesFromJson
    public static void LoadRulesFromJson(SfQueryBuilder<RuleModel> queryBuilder, string json)
    {
        if (queryBuilder == null || string.IsNullOrWhiteSpace(json))
        {
            queryBuilder?.SetRules(new List<RuleModel>());
            return;
        }

        try
        {
            var rulesList = JsonSerializer.Deserialize<List<RuleModel>>(json);
            if (rulesList != null)
            {
                foreach (var r in rulesList)
                {
                    Console.WriteLine($"Rule: Field={r.Field}, Type={r.Type}, ValueType={r.Value?.GetType()} Value={r.Value}");
                }
                queryBuilder.SetRules(rulesList);
                return;
            }
        }
        catch
        {
            // log eventueel
        }

        queryBuilder.SetRules(new List<RuleModel>());
    }
    #endregion
}
