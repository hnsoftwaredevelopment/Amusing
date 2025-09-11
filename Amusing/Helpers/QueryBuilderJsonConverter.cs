using System.Text.Json;
using System.Text.Json.Serialization;

public static class QueryBuilderJsonConverter
{
    #region Public Methods
    public static string OldToNew( string oldJson )
    {
        if ( string.IsNullOrWhiteSpace( oldJson ) || oldJson == "[]" )
        {
            return JsonSerializer.Serialize(
                new NewQueryRuleGroup { Condition = "and", Rules = new List<NewQueryRule>(), IsLocked = false },
                new JsonSerializerOptions { WriteIndented = true } );
        }

        List<OldQueryRule>? oldList;
        try
        {
            oldList = JsonSerializer.Deserialize<List<OldQueryRule>>( oldJson );
        }
        catch
        {
            return JsonSerializer.Serialize(
                new NewQueryRuleGroup { Condition = "and", Rules = new List<NewQueryRule>(), IsLocked = false },
                new JsonSerializerOptions { WriteIndented = true } );
        }

        List<NewQueryRule> newRules = new();
        int index = 0;

        foreach ( OldQueryRule old in oldList ?? Enumerable.Empty<OldQueryRule>() )
        {
            NewQueryRule newRule = ConvertRule(old);
            newRule.RuleId = $"querybuilder_group0_rule{index++}";
            newRules.Add( newRule );
        }

        NewQueryRuleGroup group = new()
        {
            Condition = "and",
            Rules = newRules,
            IsLocked = false
        };

        return JsonSerializer.Serialize( group, new JsonSerializerOptions { WriteIndented = true } );
    }
    #endregion

    #region Private Helpers
    private static readonly HashSet<string> BooleanFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "DroppedOut",
        "IsCanceled",
        "IsPaid",
        "Dressingroom",
        "Jury",
        "Infomailing"
    };

    private static NewQueryRule ConvertRule( OldQueryRule old )
    {
        string fieldName = old.Field?.Value ?? string.Empty;
        string label = old.Field?.Label ?? string.Empty;
        string op = MapOperatorToNew(old.Operator?.Value ?? string.Empty);

        object? rawValue = old.Value?.Value;

        if ( old.Value != null && old.Value.Value.ValueKind != JsonValueKind.Undefined )
        {
            JsonElement je = old.Value.Value;

            rawValue = je.ValueKind switch
            {
                JsonValueKind.String => je.GetString(),
                JsonValueKind.Number => je.GetInt32(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Array => je.EnumerateArray().Select( x => x.ToString() ).ToArray(),
                _ => null
            };
        }

        // --- Special cases ---
        if ( fieldName.Equals( "enrolled", StringComparison.OrdinalIgnoreCase ) )
        {
            fieldName = "Festival";
            op = "in"; // always force "in"
            rawValue = EnsureArray( rawValue );
        }
        else if ( fieldName.Equals( "role", StringComparison.OrdinalIgnoreCase ) ||
          fieldName.Equals( "volunteered", StringComparison.OrdinalIgnoreCase ) )
        {
            op = "in"; // force "in" as well
            rawValue = EnsureArray( rawValue );

            // Make absolutely sure it's a string[]
            if ( rawValue is not string [ ] )
            {
                rawValue = new [ ] { rawValue?.ToString() ?? string.Empty };
            }
        }

        rawValue = FixOldContactPersons( rawValue );

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

        // --- Convert numeric booleans to true/false
        if ( BooleanFields.Contains( fieldName ) )
        {
            if ( rawValue is int i )
            {
                rawValue = i != 0;
            }
            else if ( rawValue is string s )
            {
                rawValue = s == "1";
            }
        }

        return new NewQueryRule
        {
            Field = newField,
            Label = label,
            Operator = op,
            Type = DetermineType( rawValue, op, newField ),
            Value = rawValue,
            IsLocked = false
        };
    }

    private static object EnsureArray( object? value )
    {
        if ( value == null )
        {
            return Array.Empty<string>();
        }

        switch ( value )
        {
            case string s:
                return s.Split( ',', StringSplitOptions.RemoveEmptyEntries )
                        .Select( x => x.Trim() )
                        .ToArray();
            case int i:
                return new [ ] { i.ToString() };
            case string [ ] arr:
                return arr;
            case IEnumerable<string> strEnum:
                return strEnum.ToArray();
            default:
                return new [ ] { value.ToString()! };
        }
    }

    private static string DetermineType( object? value, string op, string field )
    {
        if ( op == "in" )
        {
            return "String";
        }

        if ( BooleanFields.Contains( field ) )
        {
            return "Boolean";
        }

        if ( value is bool )
        {
            return "Boolean";
        }

        if ( value is int )
        {
            return "Number";
        }

        return "String";
    }

    private static string MapOperatorToNew( string oldOp ) => oldOp switch
    {
        "eq" => "equal",
        "in" => "in",
        "gt" => "greaterthan",
        "lt" => "lessthan",
        _ => oldOp
    };

    private static object FixOldContactPersons( object value )
    {
        if ( value is string s )
        {
            if ( s.Contains( "contactpersoon" ) )
            {
                s = s.Replace( "contactpersoon", "contactpersoon1" );
            }

            if ( s.Equals( "contact", StringComparison.OrdinalIgnoreCase ) )
            {
                s = "contactpersoon1";
            }

            if ( s.Contains( "contact2" ) )
            {
                s = s.Replace( "contact2", "contactpersoon2" );
            }

            if ( s.Equals( "treasurer", StringComparison.OrdinalIgnoreCase ) )
            {
                s = "penningmeester";
            }

            if ( s.Equals( "singer", StringComparison.OrdinalIgnoreCase ) )
            {
                s = "zanger";
            }

            return s.Split( ',', StringSplitOptions.RemoveEmptyEntries ).Select( x => x.Trim() ).ToArray();
        }

        if ( value is string [ ] arr )
        {
            return arr.Select( x =>
            {
                if ( x == "contactpersoon" )
                {
                    return "contactpersoon1";
                }

                if ( x == "contact2" )
                {
                    return "contactpersoon2";
                }

                if ( x.Equals( "contact", StringComparison.OrdinalIgnoreCase ) )
                {
                    return "contactpersoon1";
                }

                if ( x == "treasurer" )
                {
                    return "penningmeester";
                }

                if ( x == "singer" )
                {
                    return "zanger";
                }
                return x;
            } ).ToArray();
        }

        return value;
    }
    #endregion

    #region Models
    private class NewQueryRuleGroup
    {
        public string Condition { get; set; }
        public List<NewQueryRule> Rules { get; set; }
        public bool IsLocked { get; set; }
    }

    public class NewQueryRule
    {
        public string Field { get; set; }
        public string Label { get; set; }
        public string Operator { get; set; }
        public string Type { get; set; }
        public object Value { get; set; }
        public string RuleId { get; set; }
        public bool IsLocked { get; set; }
    }

    public class OldQueryRule
    {
        [JsonPropertyName( "field" )]
        public OldField Field { get; set; }

        [JsonPropertyName( "operator" )]
        public OldOperator Operator { get; set; }

        [JsonPropertyName( "value" )]
        public OldValue Value { get; set; }
    }

    public class OldField
    {
        [JsonPropertyName( "value" )]
        public string Value { get; set; }

        [JsonPropertyName( "label" )]
        public string Label { get; set; }
    }

    public class OldOperator
    {
        [JsonPropertyName( "value" )]
        public string Value { get; set; }
    }

    public class OldValue
    {
        [JsonPropertyName( "value" )]
        public JsonElement Value { get; set; }

        [JsonPropertyName( "label" )]
        public string Label { get; set; }
    }
    #endregion
}
