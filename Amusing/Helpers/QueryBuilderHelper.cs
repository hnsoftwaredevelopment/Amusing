using Syncfusion.Blazor.QueryBuilder;

namespace Amusing.Helpers;

public static class QueryBuilderHelper
{
    public static List<string> CollectFields( RuleModel root )
    {
        List<string> fields = [];
        if ( root == null )
        {
            return fields;
        }

        void Traverse( RuleModel node )
        {
            if ( node == null )
            {
                return;
            }

            if ( !string.IsNullOrEmpty( node.Field ) )
            {
                fields.Add( node.Field );
            }

            if ( node.Rules != null && node.Rules.Count > 0 )
            {
                foreach ( RuleModel? child in node.Rules )
                {
                    Traverse( child );
                }
            }
        }

        // Some callers pass a wrapper root with only Rules populated
        if ( root.Rules != null && root.Rules.Count > 0 && string.IsNullOrEmpty( root.Field ) )
        {
            foreach ( RuleModel? r in root.Rules )
            {
                Traverse( r );
            }
        }
        else
        {
            Traverse( root );
        }

        return [ .. fields.Distinct( StringComparer.OrdinalIgnoreCase ) ];
    }

    public static List<string> CollectFields( IEnumerable<RuleModel> roots )
    {
        List<string> fields = [];
        if ( roots == null )
        {
            return fields;
        }

        foreach ( RuleModel r in roots )
        {
            fields.AddRange( CollectFields( r ) );
        }

        return [ .. fields.Distinct( StringComparer.OrdinalIgnoreCase ) ];
    }

    public static string DetermineQueryFromRules( RuleModel rules, string sourceChecked )
    {
        if ( rules == null )
        {
            throw new ArgumentNullException( nameof( rules ) );
        }

        // Determine queryLevel based on rules
        int queryLevel = DetermineQueryLevel(rules, sourceChecked);

        // Get the basequery
        string baseQuery = QuerySelector.GetBaseQuery(sourceChecked, queryLevel);

        // Build the WHERE-clause
        string whereClause = QueryBuilderSqlGenerator.GenerateWhereClause(rules);
        var temp = QueryBuilderSqlGenerator.AppendConditions( baseQuery, whereClause );

        return QueryBuilderSqlGenerator.AppendConditions( baseQuery, whereClause );
    }

    private static int DetermineQueryLevel( RuleModel rules, string sourceChecked )
    {
        List<string> fields = CollectFields( rules );

        if ( sourceChecked == "persons" )
        {
            if ( fields.Contains( "Festival" ) || fields.Contains( "Dressingroom" ) || fields.Contains( "Jury" ) || fields.Contains( "IsConfirmend" ) || fields.Contains( "IsSubscribed" ) || fields.Contains( "Volunteer" ) )
            {
                return 4;
            }

            if ( fields.Contains( "IsPaid" ) || fields.Contains( "IsCanceled" ) || fields.Contains( "Singers" ) )
            {
                return 3;
            }

            if ( fields.Contains( "Role") || fields.Contains( "Infomailing" ) )
            {
                return 2;
            }
        }
        else
        {
            var additionalFieldsSet = new[] { "Dressingroom", "Jury", "IsConfirmend", "IsSubscribed", "Volunteer", "IsPaid", "IsCanceled", "Singers", "Role", "Infomailing" };

            if ( fields.Contains( "Festival" ) )
            {
                if ( !additionalFieldsSet.Any( f => fields.Contains( f ) ) )
                    return 2; // only Festival

                return 3; // Festival and one or more other
            }
        }

        return 1;
    }
}