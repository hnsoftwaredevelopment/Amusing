using Microsoft.AspNetCore.Authorization;

namespace Amusing.Helpers;

public static class Policies
{
    public const string IsAdmin = "IsAdmin";
    public const string IsUser = "IsUser";

    public static AuthorizationPolicy IsAdminPolicy()
    {
        return new AuthorizationPolicyBuilder().RequireAuthenticatedUser()
                                               .RequireRole( "admin" )
                                               .Build();
    }

    public static AuthorizationPolicy IsUserPolicy()
    {
        return new AuthorizationPolicyBuilder().RequireAuthenticatedUser()
                                               .RequireRole( "user" )
                                               .Build();
    }
}
