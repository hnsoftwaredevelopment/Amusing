using Syncfusion.Blazor.QueryBuilder;

namespace Amusing.Helpers;

public static class QueryBuilderSqlGenerator
{
    public static string GenerateWhereClause( RuleModel rules )
    {
        if ( rules == null || rules.Rules == null || !rules.Rules.Any() )
        {
            return string.Empty;
        }

        return BuildCondition( rules );
    }

    private static string BuildCondition( RuleModel rule )
    {
        if ( rule.Rules != null && rule.Rules.Any() )
        {
            IEnumerable<string> conditions = rule.Rules.Select( BuildCondition );
            string op = rule.Condition.Equals("or", StringComparison.OrdinalIgnoreCase) ? " OR " : " AND ";
            return "(" + string.Join( op, conditions ) + ")";
        }

        if ( !string.IsNullOrEmpty( rule.Field ) )
        {
            string column = MapFieldToColumn(rule.Field);

            // Meerdere waarden
            if ( rule.Value is IEnumerable<object> list )
            {
                IEnumerable<string> formatted = list.Select( v => FormatValue( v, rule.Type ) );
                string op = rule.Operator.Equals("equal", StringComparison.OrdinalIgnoreCase) ? " = " : MapOperator(rule.Operator);
                return "(" + string.Join( " OR ", formatted.Select( f => $"{column}{op}{f}" ) ) + ")";
            }

            return $"{column} {MapOperator( rule.Operator )} {FormatValue( rule.Value, rule.Type )}";
        }

        return string.Empty;
    }

    public static string AppendConditions( string baseQuery, string extraConditions )
    {
        if ( string.IsNullOrWhiteSpace( extraConditions ) )
        {
            return baseQuery;
        }

        // Trim trailing semicolon
        string trimmed = baseQuery.TrimEnd();

        if ( trimmed.EndsWith( ";" ) )
        {
            trimmed = trimmed.Substring( 0, trimmed.Length - 1 );
        }

        // Als baseQuery al een WHERE bevat, plak er "AND" achter
        if ( trimmed.Contains( "WHERE", StringComparison.OrdinalIgnoreCase ) )
        {
            return $"{trimmed} AND {extraConditions}";
        }

        // Anders zelf een WHERE toevoegen
        return $"{trimmed} WHERE {extraConditions}";
    }

    private static string MapOperator( string op ) =>
        op switch
        {
            "equal" => "=",
            "notequal" => "!=",
            "greaterthan" => ">",
            "lessthan" => "<",
            "contains" => "LIKE",
            _ => "="
        };

    private static string FormatValue( object value, string type )
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

        // Strings en numerieke waarden correct quoten
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
            _ => field // fallback voor gewone velden
        };
    }
}
