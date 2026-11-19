using System.Data.Common;
using Amusing.Facturatie;

namespace Amusing.Mijn.Services;

public sealed partial class ChoirEnrollments
{
    private static async Task<Deelnamebedrag?> ParticipationAmountAsync(DbConnection connection,
        uint festival, FestivalRegistration registration, CancellationToken ct, DbTransaction? tx = null)
    {
        await using var command = Command(connection, tx, $"""
            SELECT {FestivalTariefSql.Deelnemersprijs}, p.prijs, f.btw_percentage
            FROM ah_festivals f
            JOIN (SELECT @festival AS festival_id, @headcount AS aantal_deelnemers, @stage AS podiumsoort) i
                ON i.festival_id = f.festival_id
            {FestivalTariefSql.PodiumtariefJoin}
            """);
        Add(command, "@festival", festival);
        Add(command, "@headcount", registration.Headcount);
        Add(command, "@stage", registration.PodiumType);

        await using var reader = await command.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct) || reader.IsDBNull(0) || reader.IsDBNull(1) || reader.IsDBNull(2))
            return null;

        return new(reader.GetDecimal(0), Convert.ToDecimal(reader.GetValue(1)), reader.GetDecimal(2));
    }
}
