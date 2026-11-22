namespace Amusing.Mijn.Services;

public enum ParticipationStatus { Registered, Waitlisted, Withdrawn }

public sealed record ChoirPerformance(string Start, string End, string Stage);
public sealed record ParticipationEdition(uint Id, DateOnly Date, ParticipationStatus Status,
    List<ChoirPerformance> Performances);
public sealed record ChoirParticipationHistory(string ChoirName, IReadOnlyList<ParticipationEdition> Editions, bool CanSendNoteCopy);

public sealed partial class ChoirEnrollments
{
    public async Task<ChoirParticipationHistory?> HistoryAsync(uint actor, uint group, CancellationToken ct)
    {
        await using var connection = await source.OpenConnectionAsync(ct);
        var roles = await RolesAsync(connection, null, group, false, ct);
        if (!SupportMode.CanSupport(supportContext,actor,group) && !roles.Any(role => role.Person == actor && role.Active))
            return null;

        var choir = await GroupAsync(connection, null, group, false, ct);
        if (choir is null)
            return null;

        await using var command = Command(connection, null, """
            SELECT f.festival_id, f.festivaldatum, i.wachtlijst,
                   COALESCE(CAST(i.afgehaakt AS CHAR) NOT LIKE '0000-%', 0) AS afgemeld,
                   TIME_FORMAT(t.`from`, '%H:%i'), TIME_FORMAT(t.`to`, '%H:%i'), p.naam
            FROM ah_inschrijvingen i
            JOIN ah_festivals f ON f.festival_id = i.festival_id
            LEFT JOIN (
                planner_optredens o
                JOIN ah_timetable t ON t.timeslot_id = o.tijdvak
                JOIN ah_podia p ON p.podium_id = o.podium_id
            ) ON o.festival_id = i.festival_id AND o.zanggroep_id = i.zanggroep_id
                AND f.planning_publiceren = 1 AND i.wachtlijst = 0
                AND COALESCE(CAST(i.afgehaakt AS CHAR) NOT LIKE '0000-%', 0) = 0
            WHERE i.zanggroep_id = @group
            ORDER BY f.festivaldatum DESC, f.festival_id DESC, t.`from`, o.podium_id
            """);
        Add(command, "@group", group);
        await using var reader = await command.ExecuteReaderAsync(ct);
        var editions = new List<ParticipationEdition>();
        ParticipationEdition? edition = null;
        while (await reader.ReadAsync(ct))
        {
            var festivalId = reader.GetFieldValue<uint>(0);
            if (edition?.Id != festivalId)
            {
                var status = reader.GetBoolean(3) ? ParticipationStatus.Withdrawn
                    : reader.GetBoolean(2) ? ParticipationStatus.Waitlisted : ParticipationStatus.Registered;
                edition = new(festivalId, DateOnly.FromDateTime(reader.GetDateTime(1)), status, []);
                editions.Add(edition);
            }

            if (!reader.IsDBNull(4))
                edition.Performances.Add(new(reader.GetString(4), reader.GetString(5), reader.GetString(6)));
        }

        return new(choir.Name, editions, CanSendNoteCopy(roles, actor));
    }
}
