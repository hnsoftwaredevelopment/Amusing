using System.Security.Claims;

namespace Amusing.Mijn.Services;

public static class SupportMode
{
    public const string ActorType = "beheer";
    public const string GroupClaim = "support_group";
    public static bool IsAdministrator(this ClaimsPrincipal? user) => user?.FindFirstValue("actor_type") == ActorType;

    public static bool CanSupport(IHttpContextAccessor? context, uint actor, uint group)
    {
        var user = context?.HttpContext?.User;
        return user.IsAdministrator() &&
            user!.FindFirstValue(ClaimTypes.NameIdentifier) == actor.ToString() &&
            user.FindFirstValue(GroupClaim) == group.ToString();
    }

    public static string AuditType(IHttpContextAccessor? context, uint actor, uint group) =>
        CanSupport(context, actor, group) ? ActorType : "choir_person";
}
