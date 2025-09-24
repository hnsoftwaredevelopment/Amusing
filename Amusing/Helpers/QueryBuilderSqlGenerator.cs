using System.Text.Json;

using Syncfusion.Blazor.QueryBuilder;

namespace Amusing.Helpers;

public static class QueryBuilderSqlGenerator
{
    private static int IndexOfIgnoreCase( string text, string value ) =>
        text.IndexOf( value, StringComparison.OrdinalIgnoreCase );
    private static readonly HashSet<string> InFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
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
            return;

        // Fix Not: zet null om naar false
        if ( rule.Not == null )
        {
            rule.Not = false;
        }

        // Special handling for IN fields
        if ( rule.Operator == "in" )
        {
            rule.Value = NormalizeValue( rule.Value );
        }

        // Recurse
        if ( rule.Rules != null && rule.Rules.Any() )
        {
            foreach ( var child in rule.Rules )
            {
                NormalizeRules( child );
                child.Not = null;
            }
        }
    }

    private static object? NormalizeValue( object? value )
    {
        if ( value is JsonElement el )
        {
            switch ( el.ValueKind )
            {
                case JsonValueKind.String:
                    return el.GetString();
                case JsonValueKind.Number:
                    return el.TryGetInt32( out var i ) ? i : ( object ) el.GetDouble();
                case JsonValueKind.True:
                case JsonValueKind.False:
                    return el.GetBoolean();
                case JsonValueKind.Array:
                    var list = new List<object?>();
                    foreach ( var item in el.EnumerateArray() )
                    {
                        list.Add( NormalizeValue( item ) );
                    }
                    return list.ToArray();
                case JsonValueKind.Null:
                case JsonValueKind.Undefined:
                    return null;
            }
        }

        // If it's already an array, normalize inner values
        if ( value is IEnumerable<object> enumerable && value is not string )
        {
            return enumerable.Select( NormalizeValue ).ToArray();
        }

        return value;
    }

    private static string BuildCondition( RuleModel rule )
    {
        if ( rule.Rules != null && rule.Rules.Count != 0 )
        {
            IEnumerable<string> conditions = rule.Rules.Select(BuildCondition);
            string op = rule.Condition.Equals("or", StringComparison.OrdinalIgnoreCase) ? " OR " : " AND ";
            return "(" + string.Join( op, conditions ) + ")";
        }

        if ( !string.IsNullOrEmpty( rule.Field ) )
        {
            string column = MapFieldToColumn(rule.Field);

            // Multiple values (e.g., IN operator)
            if ( rule.Value is IEnumerable<object> list && !( rule.Value is string ) )
            {
                IEnumerable<string> formatted = list.Select(v => FormatValue(v, rule.Type));
                string op = rule.Operator.Equals("equal", StringComparison.OrdinalIgnoreCase)
                                ? " = "
                                : MapOperator(rule.Operator);

                return "(" + string.Join( " OR ", formatted.Select( f => $"{column}{op}{f}" ) ) + ")";
            }

            return $"{column} {MapOperator( rule.Operator )} {FormatValue( rule.Value, rule.Type )}";
        }

        return string.Empty;
    }

    public static string AppendConditions( string baseQuery, string extraConditions )
    {
        if ( string.IsNullOrWhiteSpace( extraConditions ) )
            return baseQuery;

        string trimmed = baseQuery.TrimEnd().TrimEnd(';');

        bool hasWhere = trimmed.Contains("WHERE", StringComparison.OrdinalIgnoreCase);
        bool addedDummyWhere = false;

        if ( !hasWhere )
        {
            trimmed += " WHERE 1=1";
            addedDummyWhere = true;
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

    private static string? FormatValue( object? value, string type )
    {
        if ( value == null )
        {
            return "NULL";
        }

        if ( type.Equals( "Boolean", StringComparison.OrdinalIgnoreCase ) )
        {
            bool b = false;
            if ( value is bool boolVal )
            {
                b = boolVal;
            }
            else if ( value is string s )
            {
                bool.TryParse( s, out b );
            }

            return b ? "1" : "0";
        }

        return value is string ? $"'{value}'" : value.ToString();
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