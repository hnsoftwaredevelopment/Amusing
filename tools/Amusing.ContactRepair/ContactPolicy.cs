namespace Amusing.ContactRepair;

public sealed record ContactLink(uint PersonId, string Role, string Email, bool Active);
public sealed record ContactRepairGroup(uint GroupId, string Name, IReadOnlyList<ContactLink> Before, IReadOnlyList<ContactLink> After);
public sealed record ContactRepairPlan(string Database, DateTime CreatedUtc, IReadOnlyList<ContactRepairGroup> Groups);

public static class ContactPolicy
{
    public static bool IsContact(string role) => role is "contactpersoon1" or "contactpersoon2";
    public static string EmailKey(ContactLink link) => string.IsNullOrWhiteSpace(link.Email)
        ? "#person:" + link.PersonId : link.Email.Trim().ToLowerInvariant();
    public static ContactLink[] Ordered(IEnumerable<ContactLink> links) => links.OrderBy(l => l.PersonId).ThenBy(l => l.Role, StringComparer.Ordinal).ToArray();

    public static IReadOnlyList<ContactLink> Normalize(IReadOnlyList<ContactLink> before)
    {
        var first = before.Where(l => l.Role == "contactpersoon1").ToArray();
        var second = before.Where(l => l.Role == "contactpersoon2").ToArray();
        var existingSecondEmails = second.Select(EmailKey).ToHashSet(StringComparer.Ordinal);
        // An existing second contact takes precedence over a matching first contact,
        // but never silently invent a first contact if every candidate is shadowed.
        var candidates = (first.Length > 1 ? first.Where(l => !existingSecondEmails.Contains(EmailKey(l))) : first)
            .GroupBy(EmailKey).Select(g => g.MaxBy(l => l.PersonId)!).ToArray();
        if (candidates.Length == 0) throw new InvalidOperationException("Geen eerste contactpersoon over na toepassing van de regels.");
        var keeper = candidates.MaxBy(l => l.PersonId)!;
        var demoted = candidates.Where(l => l.PersonId != keeper.PersonId).Select(l => l with { Role = "contactpersoon2" });
        var keptSeconds = second.Concat(demoted).GroupBy(EmailKey).Select(g => g.MaxBy(l => l.PersonId)!);
        var after = Ordered(before.Where(l => !IsContact(l.Role)).Concat([keeper]).Concat(keptSeconds));
        if (!after.Any(l => l.Role == "contactpersoon1" && l.Active))
            throw new InvalidOperationException("Geen actieve eerste contactpersoon over.");
        var beforeEmails = before.Where(l => IsContact(l.Role)).Select(EmailKey).ToHashSet(StringComparer.Ordinal);
        var afterEmails = after.Where(l => IsContact(l.Role)).Select(EmailKey).ToHashSet(StringComparer.Ordinal);
        if (!beforeEmails.SetEquals(afterEmails)) throw new InvalidOperationException("Een uniek contactadres zou wegvallen.");
        var reachableBefore = before.Where(l => IsContact(l.Role) && l.Active).Select(EmailKey);
        var reachableAfter = after.Where(l => IsContact(l.Role) && l.Active).Select(EmailKey).ToHashSet(StringComparer.Ordinal);
        if (reachableBefore.Any(key => !reachableAfter.Contains(key)))
            throw new InvalidOperationException("Een actief contactadres zou alleen nog aan een inactief account gekoppeld zijn.");
        return after;
    }
}
