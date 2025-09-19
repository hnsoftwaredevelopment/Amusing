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
            return queryLevel switch
            {
                1 => QueryDefinitions.GetGroupsList,
                2 => QueryDefinitions.GetGroupsWithFestivalList,
                3 => QueryDefinitions.GetFullGroupsList,
                _ => QueryDefinitions.GetGroupsList
            };
        }

        throw new InvalidOperationException( "Unknown SourceChecked value: " + sourceChecked );
    }
}
