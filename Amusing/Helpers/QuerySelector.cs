namespace Amusing.Helpers;

public static class QuerySelector
{
    public static string GetBaseQuery( string sourceChecked, int queryLevel )
    {
        if ( sourceChecked == "persons" )
        {
            return queryLevel switch
            {
                1 => QueryDefinitions.GetPersonsList,
                2 => QueryDefinitions.GetPersonsWithRoleAndGroupList,
                3 => QueryDefinitions.GetPersonsWithRoleAndGroupAndSubscriptionList,
                4 => QueryDefinitions.GetFullPersonsList,
                _ => QueryDefinitions.GetPersonsList
            };
        }
        else if ( sourceChecked == "groups" )
        {
            // Later uitbreiden met je eigen group-queries
            //return queryLevel switch
            //{
            //    1 => QueryDefinitions.GetGroupsList,
            //    2 => QueryDefinitions.GetGroupsWithSomething,
            //    _ => QueryDefinitions.GetGroupsList
            //};
        }

        throw new InvalidOperationException( "Unknown SourceChecked value: " + sourceChecked );
    }
}
