using Amusing.Shared;
namespace Amusing.Mijn.Services;

public sealed partial class ChoirEnrollments
{
    private static bool ResolveAvailability(FestivalEdition festival, string? fromText, string? untilText,
        FestivalRegistration? current, out TimeSpan from, out TimeSpan until)
    {
        from = current?.AvailableFrom ?? festival.FirstPerformance;
        until = current?.AvailableUntil ?? festival.LastPerformance;
        // Unchanged historical intervals remain intact on unrelated registration changes.
        if (fromText is null && untilText is null) return current is not null || festival.Availability.Allows(from, until);
        return FestivalAvailability.TryTime(fromText, out from) && FestivalAvailability.TryTime(untilText, out until)
            && (current is not null && from == current.AvailableFrom && until == current.AvailableUntil
                || festival.Availability.Allows(from, until));
    }

    public async Task<EnrollmentResult> UpdateAvailabilityAsync(uint actor, uint group, EnrollmentAvailabilityInput input, CancellationToken ct)
    {
        await using var connection = await source.OpenConnectionAsync(ct);
        await using var tx = await connection.BeginTransactionAsync(ct);
        var roles = await RolesAsync(connection, tx, group, true, ct);
        if (!SupportMode.CanSupport(supportContext,actor,group) && !Contact(roles,actor)) return EnrollmentResult.Forbidden;
        if (await GroupAsync(connection,tx,group,true,ct) is not { Active:true }) return EnrollmentResult.Forbidden;
        var festival = await FestivalAsync(connection,tx,DateOnly.FromDateTime(clock.Now),true,ct);
        if (festival is null || festival.Id != input.FestivalId) return EnrollmentResult.EditionChanged;
        var current = await RegistrationAsync(connection,tx,festival.Id,group,ct,locked:true);
        if (current is null) return EnrollmentResult.MissingRegistration;
        if (current.Cancelled) return EnrollmentResult.Locked;
        if (current.Version != input.Version) return EnrollmentResult.Conflict;
        if (input.AvailableFrom is null || input.AvailableUntil is null ||
            !ResolveAvailability(festival,input.AvailableFrom,input.AvailableUntil,current,out var from,out var until)) return EnrollmentResult.Invalid;
        if (from == current.AvailableFrom && until == current.AvailableUntil) return EnrollmentResult.Saved;
        await using var update = Command(connection,tx,"UPDATE ah_inschrijvingen SET beschikbaar_van=@from,beschikbaar_tot=@until WHERE festival_id=@festival AND zanggroep_id=@group");
        Add(update,"@from",from);Add(update,"@until",until);Add(update,"@festival",festival.Id);Add(update,"@group",group);
        await update.ExecuteNonQueryAsync(ct);
        await using var audit = Command(connection,tx,"""
            INSERT INTO mijn_audit_log(occurred_at,actor_type,actor_id,action,group_id,festival_id,changed_fields)
            VALUES(UTC_TIMESTAMP(6),@actorType,@actor,'FestivalEnrollmentUpdated',@group,@festival,'AvailableFrom,AvailableUntil')
            """);
        Add(audit,"@actorType",SupportMode.AuditType(supportContext,actor,group));Add(audit,"@actor",actor);
        Add(audit,"@group",group);Add(audit,"@festival",festival.Id);await audit.ExecuteNonQueryAsync(ct);
        await tx.CommitAsync(ct);
        return EnrollmentResult.Saved;
    }
}
