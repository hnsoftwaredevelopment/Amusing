using System.Text.Json;

namespace Amusing.Helpers;

public static class QueryBuilderJsonConverter
{
    #region Public Methods

    public static string NewToOld(string newJson)
    {
        var newRoot = JsonSerializer.Deserialize<NewQueryRoot>(newJson);
        var oldList = new List<OldQueryRule>();

        if (newRoot?.Rules == null) return "[]";

        foreach (var rule in newRoot.Rules)
        {
            oldList.Add(new OldQueryRule
            {
                field = new { label = rule.Label, value = rule.Field },
                operatorProp = new
                {
                    label = MapOperatorLabel(rule.Operator),
                    value = MapOperatorValue(rule.Operator)
                },
                value = new
                {
                    label = FormatValueLabel(rule.Value),
                    value = FormatValueValue(rule.Value)
                }
            });
        }

        return JsonSerializer.Serialize(oldList);
    }

    public static string OldToNew(string oldJson)
    {
        var oldList = JsonSerializer.Deserialize<List<OldQueryRule>>(oldJson);
        var newRoot = new NewQueryRoot { Condition = "and", Rules = new List<NewQueryRule>() };

        if (oldList == null) return JsonSerializer.Serialize(newRoot);

        foreach (var oldRule in oldList)
        {
            newRoot.Rules.Add(new NewQueryRule
            {
                Field = oldRule.field.value,
                Label = oldRule.field.label,
                Operator = MapOperatorToNew(oldRule.operatorProp.value),
                Type = DetectType(oldRule),
                Value = ParseValue(oldRule)
            });
        }

        return JsonSerializer.Serialize(newRoot, new JsonSerializerOptions { WriteIndented = true });
    }

    #endregion

    #region Helper Methods

    private static string MapOperatorLabel(string op)
    {
        return op switch
        {
            "equal" => "equals",
            "in" => "any of",
            "greaterthan" => ">",
            "lessthan" => "<",
            _ => op
        };
    }

    private static string MapOperatorValue(string op)
    {
        return op switch
        {
            "equal" => "eq",
            "in" => "in",
            "greaterthan" => "gt",
            "lessthan" => "lt",
            _ => op
        };
    }

    private static string FormatValueLabel(object value)
    {
        if (value is bool b) return b ? "Yes" : "No";
        if (value is string s) return s;
        if (value is JsonElement je && je.ValueKind == JsonValueKind.Array)
            return "(" + string.Join(", ", je.EnumerateArray().Select(x => x.ToString())) + ")";
        return value?.ToString() ?? "";
    }

    private static object FormatValueValue(object value)
    {
        if (value is bool b) return b ? 1 : 0;
        if (value is JsonElement je && je.ValueKind == JsonValueKind.Array)
            return string.Join(",", je.EnumerateArray().Select(x => x.ToString()));
        return value?.ToString() ?? "";
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

    private static string DetectType(OldQueryRule rule)
    {
        if (rule.value.value is int || rule.value.value is bool) return "Boolean";
        if (rule.value.value is string s && s.Contains(",")) return "String";
        if (rule.value.value is string) return "Number";
        return "String";
    }

    private static object ParseValue(OldQueryRule rule)
    {
        if (rule.value.value is int i) return i;
        if (rule.value.value is bool b) return b;
        if (rule.value.value is string s && s.Contains(","))
            return s.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();
        if (rule.value.value is string s2)
        {
            if (int.TryParse(s2, out var num)) return num;
            if (s2 == "Yes") return true;
            if (s2 == "No") return false;
            return s2;
        }

        return rule.value.value;
    }

    #endregion

    #region Models

    public class OldQueryRule
    {
        public dynamic field { get; set; }
        public dynamic operatorProp { get; set; }  // operator is reserved
        public dynamic value { get; set; }
    }

    public class NewQueryRoot
    {
        public string Condition { get; set; }
        public List<NewQueryRule> Rules { get; set; }
    }

    public class NewQueryRule
    {
        public string Field { get; set; }
        public string Label { get; set; }
        public string Operator { get; set; }
        public string Type { get; set; }
        public object Value { get; set; }
    }

    #endregion
}
