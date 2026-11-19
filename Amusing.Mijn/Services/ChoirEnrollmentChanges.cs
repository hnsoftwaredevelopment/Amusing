using System.ComponentModel.DataAnnotations;
namespace Amusing.Mijn.Services;

public sealed partial class ChoirEnrollments
{
    public Task<EnrollmentResult> UpdateAsync(uint actor, uint group, EnrollmentInput input, CancellationToken ct)
        => ChangeAsync(actor, group, input.FestivalId, input.Version, input, ct);

    public Task<EnrollmentResult> ConfirmAsync(uint actor, uint group, EnrollmentConfirmation input, CancellationToken ct)
        => ChangeAsync(actor, group, input.FestivalId, input.Version, null, ct);

    private async Task<EnrollmentResult> ChangeAsync(uint actor, uint group, uint festivalId, string? version, EnrollmentInput? edit, CancellationToken ct)
    {
        await using var connection = await source.OpenConnectionAsync(ct);
        await using var tx = await connection.BeginTransactionAsync(ct);
        var roles = await RolesAsync(connection, tx, group, true, ct);
        if (!SupportMode.CanSupport(supportContext,actor,group) && !Contact(roles, actor)) return EnrollmentResult.Forbidden;
        var choir = await GroupAsync(connection, tx, group, true, ct);
        if (choir is not { Active: true }) return EnrollmentResult.Forbidden;

        var festival = await FestivalAsync(connection, tx, DateOnly.FromDateTime(clock.Now), true, ct);
        if (festival is null || festival.Id != festivalId) return EnrollmentResult.EditionChanged;
        var current = await RegistrationAsync(connection, tx, festival.Id, group, ct, locked: true);
        if (current is null) return EnrollmentResult.MissingRegistration;
        if (current.Cancelled) return EnrollmentResult.Locked;

        var now = clock.Now;
        var today = DateOnly.FromDateTime(now);
        if (today > festival.Date) return EnrollmentResult.EditionChanged;
        if (edit is null)
        {
            if (current.Confirmed) return EnrollmentResult.AlreadyConfirmed;
            if (current.Waitlisted) return EnrollmentResult.Waitlisted;
            if (today <= festival.End) return EnrollmentResult.TooEarly;
        }
        else if (!current.CanEdit)
        {
            return EnrollmentResult.Locked;
        }

        if (version != current.Version) return EnrollmentResult.Conflict;
        string action;
        string fields;
        if (edit is null)
        {
            await using var confirm = Command(connection, tx,
                "UPDATE ah_inschrijvingen SET bevestigd=@now WHERE festival_id=@festival AND zanggroep_id=@group");
            Add(confirm, "@now", now);
            Add(confirm, "@festival", festivalId);
            Add(confirm, "@group", group);
            await confirm.ExecuteNonQueryAsync(ct);
            var amount = await ParticipationAmountAsync(connection, festivalId, current, ct, tx);
            if (amount is not null)
                await SaveNoteAsync(connection, tx, group, festival, choir.Name, current, today, amount, ct);
            action = "FestivalEnrollmentConfirmed";
            fields = "Confirmed";
        }
        else
        {
            edit.PodiumType = edit.PodiumType?.Trim().ToUpperInvariant() ?? "";
            if (!Validator.TryValidateObject(edit, new ValidationContext(edit), new List<ValidationResult>(), true))
                return EnrollmentResult.Invalid;
            if (edit.PodiumType != current.PodiumType &&
                (current.ForcedStage || !(await StagesAsync(connection, tx, festivalId, ct)).Any(stage => stage.Id == edit.PodiumType)))
                return EnrollmentResult.Invalid;

            var changes = new List<string>();
            if (edit.Headcount != current.Headcount) changes.Add("Headcount");
            if (edit.PodiumType != current.PodiumType) changes.Add("PodiumType");
            if (edit.Review != current.Review) changes.Add("Review");
            int indoors = edit.Preference[0] - '0', outdoors = edit.Preference[1] - '0';
            if (indoors != current.Indoors || outdoors != current.Outdoors) changes.Add("Preference");
            if (changes.Count == 0) return EnrollmentResult.Saved;

            await using var update = Command(connection, tx, """
                UPDATE ah_inschrijvingen
                SET aantal_deelnemers=@count,podiumsoort=@stage,wens_4=@review,binnenoptredens=@inside,buitenoptredens=@outside
                WHERE festival_id=@festival AND zanggroep_id=@group
                """);
            Add(update, "@count", edit.Headcount);
            Add(update, "@stage", edit.PodiumType);
            Add(update, "@review", edit.Review ? "ja" : "nee");
            Add(update, "@inside", indoors);
            Add(update, "@outside", outdoors);
            Add(update, "@festival", festivalId);
            Add(update, "@group", group);
            await update.ExecuteNonQueryAsync(ct);
            if (changes.Contains("Headcount") || changes.Contains("PodiumType"))
            {
                var updated = current with { Headcount = (uint)edit.Headcount, PodiumType = edit.PodiumType,
                    Review = edit.Review, Indoors = indoors, Outdoors = outdoors };
                await QueueEnrollmentMailAsync(connection,tx,festival,group,choir.Name,updated,ct,noteOnly:true);
            }
            action = "FestivalEnrollmentUpdated";
            fields = string.Join(",", changes);
        }

        await using var audit = Command(connection, tx, """
            INSERT INTO mijn_audit_log(occurred_at,actor_type,actor_id,action,group_id,festival_id,changed_fields)
            VALUES(UTC_TIMESTAMP(6),@actorType,@actor,@action,@group,@festival,@fields)
            """);
        Add(audit, "@actorType", SupportMode.AuditType(supportContext,actor,group));
        Add(audit, "@actor", actor);
        Add(audit, "@action", action);
        Add(audit, "@group", group);
        Add(audit, "@festival", festivalId);
        Add(audit, "@fields", fields);
        await audit.ExecuteNonQueryAsync(ct);
        await tx.CommitAsync(ct);
        return edit is null ? EnrollmentResult.Confirmed : EnrollmentResult.Saved;
    }
}
