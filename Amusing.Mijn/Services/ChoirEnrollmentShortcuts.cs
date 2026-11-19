namespace Amusing.Mijn.Services;

public sealed record EnrollmentShortcuts(FestivalEdition Festival, HashSet<uint> Choirs);

public sealed partial class ChoirEnrollments
{
    public async Task<EnrollmentShortcuts?> ShortcutsAsync(uint actor, CancellationToken ct)
    {
        await using var connection = await source.OpenConnectionAsync(ct);
        var today = DateOnly.FromDateTime(clock.Now);
        var festival = await FestivalAsync(connection, null, today, false, ct);
        if (festival is null || !festival.IsOpen(today)) return null;

        await using var cmd = Command(connection, null, """
            SELECT DISTINCT g.zanggroep_id
            FROM ah_zanggroepen g
            JOIN ah_personen_rollen r USING (zanggroep_id)
            JOIN ah_personen p USING (persoon_id)
            WHERE g.actief=1 AND p.actief=1 AND p.persoon_id=@actor
              AND r.rol IN ('contactpersoon1','contactpersoon2')
              AND NOT EXISTS (SELECT 1 FROM ah_inschrijvingen i
                  WHERE i.festival_id=@festival AND i.zanggroep_id=g.zanggroep_id)
            """);
        Add(cmd, "@actor", actor);
        Add(cmd, "@festival", festival.Id);
        var groups = new HashSet<uint>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct)) groups.Add(reader.GetFieldValue<uint>(0));
        return new(festival, groups);
    }
}
