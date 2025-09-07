using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Amusing.Helpers
{
    public static class QueryBuilderJsonConverter
    {
        #region Public Methods

        /// <summary>
        /// Convert old JSON array format (from legacy app) to the new QueryBuilder root JSON.
        /// Returns a JSON string that matches the new structure.
        /// </summary>
        public static string OldToNew(string oldJson)
        {
            if (string.IsNullOrWhiteSpace(oldJson) || oldJson == "[]")
                return JsonSerializer.Serialize(new NewQueryRoot { Condition = "and", Rules = new List<NewQueryRule>() }, new JsonSerializerOptions { WriteIndented = true });

            List<OldQueryRule> oldList;
            try
            {
                oldList = JsonSerializer.Deserialize<List<OldQueryRule>>(oldJson);
            }
            catch
            {
                // If we cannot parse old JSON, return an empty root
                return JsonSerializer.Serialize(new NewQueryRoot { Condition = "and", Rules = new List<NewQueryRule>() }, new JsonSerializerOptions { WriteIndented = true });
            }

            var newRoot = new NewQueryRoot { Condition = "and", Rules = new List<NewQueryRule>() };

            foreach (var old in oldList)
            {
                var op = MapOperatorToNew(old.@operator?.value ?? string.Empty);
                var parsedValue = ParseValue(old);

                // --- Map old field name to new QueryBuilder Field ---
                string newField = old.field?.value switch
                {
                    "enrolled" => "Festival",
                    "role" => "Role",
                    "volunteered" => "Volunteer",
                    "mailing" => "Infomailing",
                    "droppedout" => "IsCanceled",
                    "payed" => "IsPaid",
                    "review" => "Jury",
                    "headcount" => "Singers",
                    "dressingroom" => "Dressingroom",
                    _ => old.field?.value ?? string.Empty
                };

                // --- Fix old contactpersonen ---
                parsedValue = FixOldContactPersons(parsedValue);

                // --- Ensure array for "in" operators ---
                if (op == "in")
                {
                    if (parsedValue is string s)
                    {
                        parsedValue = s.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                       .Select(x => x.Trim())
                                       .ToArray();
                    }
                    else if (parsedValue is int i)
                    {
                        parsedValue = new[] { i.ToString() };
                    }
                    else if (parsedValue is Array == false)
                    {
                        parsedValue = new[] { parsedValue.ToString() };
                    }
                }

                if (newField.Equals("Festival", StringComparison.OrdinalIgnoreCase) ||
                    newField.Equals("Role", StringComparison.OrdinalIgnoreCase) ||
                    newField.Equals("Volunteer", StringComparison.OrdinalIgnoreCase))
                {
                    op = "in"; // altijd multi-select

                    // Ensure the value is an array of strings
                    if (parsedValue is int i)
                        parsedValue = new[] { i.ToString() };
                    else if (parsedValue is string s)
                        parsedValue = s.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                       .Select(x => x.Trim())
                                       .ToArray();
                    else if (parsedValue is not Array)
                        parsedValue = new[] { parsedValue.ToString() };
                }

                // Determine type
                var inferredType = DetectTypeFromParsed(parsedValue, newField);

                // --- Add only once ---
                newRoot.Rules.Add(new NewQueryRule
                {
                    Field = newField,
                    Label = old.field?.label ?? string.Empty,
                    Operator = op,
                    Type = inferredType,
                    Value = parsedValue
                });
            }

            return JsonSerializer.Serialize(newRoot, new JsonSerializerOptions { WriteIndented = true });
        }

        /// <summary>
        /// Convert new JSON root (or RuleModel-like structure) back to the old legacy array format.
        /// Useful when saving for the legacy consumer.
        /// </summary>
        public static string NewToOld(string newJson)
        {
            if (string.IsNullOrWhiteSpace(newJson))
                return "[]";

            NewQueryRoot? root = null;
            try
            {
                root = JsonSerializer.Deserialize<NewQueryRoot>(newJson);
            }
            catch
            {
                // ignore
            }

            var oldList = new List<OldQueryRuleOut>();

            if (root?.Rules == null || root.Rules.Count == 0)
                return "[]";

            foreach (var rule in root.Rules)
            {
                // format value for old representation
                var (label, value) = FormatValueForOld(rule.Value);

                oldList.Add(new OldQueryRuleOut
                {
                    field = new OldFieldOut { label = rule.Label ?? string.Empty, value = rule.Field ?? string.Empty },
                    @operator = new OldOperatorOut { label = MapOperatorLabel(rule.Operator), value = MapOperatorValue(rule.Operator) },
                    value = new OldValueOut { label = label, value = value }
                });
            }

            var options = new JsonSerializerOptions { WriteIndented = false };
            return JsonSerializer.Serialize(oldList, options);
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

        private static string DetectTypeFromParsed(object value, string fieldName = "")
        {
            // Special mapping based on field name
            switch (fieldName)
            {
                case "IsPaid":
                case "IsCanceled":
                case "Dressingroom":
                case "Jury":
                case "Infomailing":
                    return "Boolean";

                case "Festival":
                case "Role":
                case "Volunteer":
                    return "String";

                case "Singers":
                    return "Number";
            }

            // Fallback: infer type from value
            if (value is bool) return "Boolean";
            if (value is int || value is long || value is double || value is float) return "Number";
            if (value is string) return "String";
            if (value is Array) return "String"; // JSON array of strings
            return "String";
        }

        private static object ParseValue(OldQueryRule rule)
        {
            if (rule?.value == null)
                return string.Empty;

            var je = rule.value.value; // JsonElement

            switch (je.ValueKind)
            {
                case JsonValueKind.Array:
                    // return string[]
                    return je.EnumerateArray()
                             .Select(x => x.ValueKind == JsonValueKind.String ? x.GetString()! : x.GetRawText().Trim('"'))
                             .Where(s => !string.IsNullOrEmpty(s))
                             .ToArray();

                case JsonValueKind.Number:
                    if (je.TryGetInt32(out var n)) return n;
                    if (je.TryGetDouble(out var d)) return d;
                    return je.GetRawText();

                case JsonValueKind.True:
                    return true;
                case JsonValueKind.False:
                    return false;

                case JsonValueKind.String:
                    var s = je.GetString() ?? string.Empty;
                    // Old format sometimes used CSV in a string: "2026,2025"
                    if (s.Contains(","))
                        return s.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();

                    // Boolean textuals
                    if (s.Equals("Yes", StringComparison.OrdinalIgnoreCase) || s.Equals("Ja", StringComparison.OrdinalIgnoreCase) || s == "1")
                        return true;
                    if (s.Equals("No", StringComparison.OrdinalIgnoreCase) || s.Equals("Nee", StringComparison.OrdinalIgnoreCase) || s == "0")
                        return false;

                    if (int.TryParse(s, out var pi)) return pi;
                    if (double.TryParse(s, out var pd)) return pd;

                    return s;

                default:
                    return je.GetRawText();
            }
        }

        private static (string label, object value) FormatValueForOld(object? newValue)
        {
            if (newValue is null) return (string.Empty, string.Empty);

            if (newValue is bool b) return (b ? "Yes" : "No", b ? 1 : 0);

            if (newValue is int i) return (i.ToString(), i);

            if (newValue is IEnumerable<string> sa)
            {
                var arr = sa.ToArray();
                var label = "(" + string.Join(", ", arr) + ")";
                var value = string.Join(",", arr);
                return (label, value);
            }

            if (newValue is string s) return (s, s);

            // fallback
            return (newValue.ToString() ?? string.Empty, newValue.ToString() ?? string.Empty);
        }

        #endregion

        #region Old (input) typed models

        // These types match the legacy JSON structure
        public class OldQueryRule
        {
            public OldField field { get; set; } = new OldField();
            [JsonPropertyName("operator")]
            public OldOperator @operator { get; set; } = new OldOperator();
            public OldValue value { get; set; } = new OldValue();
        }

        public class OldField
        {
            public string label { get; set; } = string.Empty;
            public string value { get; set; } = string.Empty;
        }

        public class OldOperator
        {
            public string label { get; set; } = string.Empty;
            public string value { get; set; } = string.Empty;
        }

        public class OldValue
        {
            public string label { get; set; } = string.Empty;
            public JsonElement value { get; set; } // flexible: can be number/string/array/boolean
        }

        #endregion

        #region Old (output) typed models for NewToOld

        // These are used when converting NEW -> OLD format for backward save
        public class OldQueryRuleOut
        {
            public OldFieldOut field { get; set; } = new OldFieldOut();
            [JsonPropertyName("operator")]
            public OldOperatorOut @operator { get; set; } = new OldOperatorOut();
            public OldValueOut value { get; set; } = new OldValueOut();
        }

        public class OldFieldOut { public string label { get; set; } = string.Empty; public string value { get; set; } = string.Empty; }
        public class OldOperatorOut { public string label { get; set; } = string.Empty; public string value { get; set; } = string.Empty; }
        public class OldValueOut { public string label { get; set; } = string.Empty; public object value { get; set; } = string.Empty; }

        #endregion

        #region New typed models (target structure)

        // NewQueryRoot / NewQueryRule are simple DTOs that mimic the Syncfusion RuleModel structure.
        public class NewQueryRoot
        {
            public string Condition { get; set; } = "and";
            public List<NewQueryRule> Rules { get; set; } = new List<NewQueryRule>();
        }

        public class NewQueryRule
        {
            public string? Condition { get; set; }
            public string? Field { get; set; }
            public string? Label { get; set; }
            public string? Operator { get; set; }
            public string? Type { get; set; }
            public object? Value { get; set; }
            public List<NewQueryRule>? Rules { get; set; }
            public string? RuleId { get; set; }
            public bool IsLocked { get; set; }
        }

        #endregion

        #region Helper: fix "contactpersoon" -> "contactpersoon1"
        private static object FixOldContactPersons(object value)
        {
            var mapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "contact", "contactpersoon1" },
                { "contact2", "contactpersoon2" }
            };

            if(value is string s)
            {
                return mapping.ContainsKey(s) ? mapping[s] : s;
            }

            if (value is string[] arr)
            {
                for (int i = 0; i < arr.Length; i++)
                {
                    if (mapping.ContainsKey(arr[i]))
                        arr[i] = mapping[arr[i]];
                }
                return arr;
            }

            return value;
        }
        #endregion
    }
}
