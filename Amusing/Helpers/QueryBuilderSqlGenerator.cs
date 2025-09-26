using System.Text.Json;

using Syncfusion.Blazor.QueryBuilder;

namespace Amusing.Helpers;

public static class QueryBuilderSqlGenerator
{
    // Fields that should always be treated as "IN" (can be extended)
    private static readonly HashSet<string> InFields = new(StringComparer.OrdinalIgnoreCase)
        {
            "Festival", "Role", "Volunteer"
        };


    public static string GenerateWhereClause( RuleModel rules )
    {
        if ( rules == null || rules.Rules == null || rules.Rules.Count == 0 )
        {
            return string.Empty;
        }

        // Always normalize before building conditions
        NormalizeRules( rules );

        return BuildCondition( rules );
    }

    private static void NormalizeRules( RuleModel? rule )
    {
        if ( rule == null )
        {
            return;
        }

        // Normalize OPERATOR
        if ( rule.Operator is JsonElement jeOp )
        {
            rule.Operator = jeOp.ValueKind == JsonValueKind.String ? jeOp.GetString() : jeOp.GetRawText();
        }
        else
        {
            rule.Operator = rule.Operator?.ToString();
        }

        // Normalize VALUE (convert JsonElement, stringified arrays, collections)
        rule.Value = NormalizeValue( rule.Value, rule.Type );

        // Force arrays for known IN-fields
        if ( !string.IsNullOrEmpty( rule.Field ) && InFields.Contains( rule.Field ) )
        {
            if ( rule.Value == null )
            {
                rule.Value = Array.Empty<object>();
            }
            else if ( rule.Value is string )
            {
                rule.Value = new object [ ] { rule.Value };
            }
            else if ( rule.Value is not System.Collections.IEnumerable )
            {
                rule.Value = new object [ ] { rule.Value };
            }
        }

        // Recurse into children
        if ( rule.Rules != null && rule.Rules.Any() )
        {
            foreach ( RuleModel? child in rule.Rules )
            {
                NormalizeRules( child );
            }
        }
    }

    private static object? NormalizeValue( object? value, string? type )
    {
        if ( value == null )
        {
            return null;
        }

        // Handle JsonElement values
        if ( value is JsonElement je )
        {
            switch ( je.ValueKind )
            {
                case JsonValueKind.String:
                    return je.GetString();

                case JsonValueKind.Number:
                    if ( je.TryGetInt32( out int i ) )
                    {
                        return i;
                    }

                    if ( je.TryGetInt64( out long l ) )
                    {
                        return l;
                    }

                    if ( je.TryGetDouble( out double d ) )
                    {
                        return d;
                    }

                    return je.GetRawText();

                case JsonValueKind.True:
                case JsonValueKind.False:
                    return je.GetBoolean();

                case JsonValueKind.Array:
                    List<object?> arr = new();
                    foreach ( JsonElement child in je.EnumerateArray() )
                    {
                        arr.Add( NormalizeValue( child, type ) );
                    }
                    return arr.ToArray();

                case JsonValueKind.Null:
                case JsonValueKind.Undefined:
                default:
                    return null;
            }
        }

        // Try to parse stringified JSON arrays
        if ( value is string s )
        {
            string t = s.Trim();
            if ( t.StartsWith( "[" ) && t.EndsWith( "]" ) )
            {
                try
                {
                    string [ ]? maybe = JsonSerializer.Deserialize<string[]>(t);
                    if ( maybe != null )
                    {
                        return maybe;
                    }

                    object [ ]? maybeObj = JsonSerializer.Deserialize<object[]>(t);
                    if ( maybeObj != null )
                    {
                        return maybeObj;
                    }
                }
                catch
                {
                    // Ignore and return raw string
                }
            }

            return s;
        }

        // Normalize IEnumerable (non-string) values recursively
        if ( value is System.Collections.IEnumerable enumerable && !( value is string ) )
        {
            List<object?> tmp = new();
            foreach ( object? item in enumerable )
            {
                tmp.Add( NormalizeValue( item, type ) );
            }
            return tmp.ToArray();
        }

        // Primitive types (already normalized)
        return value;
    }

    private static string BuildCondition( RuleModel rule )
    {
        if ( rule == null )
        {
            return string.Empty;
        }

        // Composite node with child rules
        if ( rule.Rules != null && rule.Rules.Count != 0 )
        {
            List<string> conditions = rule.Rules
            .Select( BuildCondition )
            .Where( x => !string.IsNullOrWhiteSpace( x ) )
            .ToList();

            string cond = (rule.Condition ?? "and").ToString();
            string joinOp = string.Equals(cond, "or", StringComparison.OrdinalIgnoreCase) ? " OR " : " AND ";

            return conditions.Any() ? "(" + string.Join( joinOp, conditions ) + ")" : string.Empty;
        }

        // Leaf node (single condition)
        if ( string.IsNullOrEmpty( rule.Field ) )
        {
            return string.Empty;
        }

        string column = MapFieldToColumn(rule.Field);
        string operatorValue = (rule.Operator ?? "equal").ToString();

        // Handle array values as IN/NOT IN
        if ( rule.Value is System.Collections.IEnumerable valEnum && !( rule.Value is string ) )
        {
            List<string> items = new();
            foreach ( object? v in valEnum )
            {
                string? formatted = FormatValue( v, rule.Type );
                if ( !string.IsNullOrEmpty( formatted ) )
                {
                    items.Add( formatted );
                }
            }

            if ( !items.Any() )
            {
                return string.Empty;
            }

            if ( string.Equals( operatorValue, "notequal", StringComparison.OrdinalIgnoreCase ) )
            {
                return $"{column} NOT IN ({string.Join( ", ", items )})";
            }
            else
            {
                return $"{column} IN ({string.Join( ", ", items )})";
            }
        }

        // Handle single value
        dynamic single = FormatValue(rule.Value, rule.Type);
        if ( string.IsNullOrEmpty( single ) )
        {
            return string.Empty;
        }

        return $"{column} {MapOperator( operatorValue )} {single}";
    }

    public static string AppendConditions( string baseQuery, string extraConditions )
    {
        if ( string.IsNullOrWhiteSpace( extraConditions ) )
        {
            return baseQuery;
        }

        string trimmed = baseQuery.TrimEnd().TrimEnd(';');

        bool hasWhere = trimmed.Contains("WHERE", StringComparison.OrdinalIgnoreCase);

        if ( !hasWhere )
        {
            trimmed += " WHERE 1=1";
        }

        string result = $"{trimmed} AND {extraConditions};";

        result = result.Replace( "WHERE 1=1 AND ", "WHERE " );

        return result;
    }

    private static string MapOperator( string op ) =>
        op switch
        {
            "equal" => "=",
            "notequal" => "!=",
            "greaterthan" => ">",
            "lessthan" => "<",
            "contains" => "LIKE",
            "in" => "=", // handled as OR chain
            _ => "="
        };

    private static string? FormatValue( object? value, string? type )
    {
        if ( value == null )
        {
            return "NULL";
        }

        // Boolean values -> return 1 or 0
        if ( !string.IsNullOrEmpty( type ) && type.Equals( "Boolean", StringComparison.OrdinalIgnoreCase ) )
        {
            bool b = false;
            if ( value is bool bb )
            {
                b = bb;
            }
            else if ( value is string s )
            {
                bool.TryParse( s, out b );
            }
            else if ( value is int iv )
            {
                b = iv != 0;
            }

            return b ? "1" : "0";
        }

        // Numeric values -> no quotes
        if ( value is int || value is long || value is double || value is float || value is decimal )
        {
            return value.ToString();
        }

        // Strings -> escape single quotes
        string str = value.ToString() ?? string.Empty;
        str = str.Replace( "'", "''" );
        return $"'{str}'";
    }


    private static string MapFieldToColumn( string field )
    {
        return field switch
        {
            "Festival" => QueryDefinitions.WhereFestival,
            "IsPaid" => QueryDefinitions.WherePaid,
            "IsCanceled" => QueryDefinitions.WhereCanceled,
            "Dressingroom" => QueryDefinitions.WhereDressingroom,
            "Jury" => QueryDefinitions.WhereJury,
            "Singers" => QueryDefinitions.WhereSingers,
            "Volunteer" => QueryDefinitions.WhereVolunteer,
            "IsSubscribed" => QueryDefinitions.WhereSubscribed,
            "Confirmed" => QueryDefinitions.WhereConfirmed,
            "Infomailing" => QueryDefinitions.WhereInfomailing,
            "Role" => QueryDefinitions.WhereRole,
            _ => field
        };
    }
}