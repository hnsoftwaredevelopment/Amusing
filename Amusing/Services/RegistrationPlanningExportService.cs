using System.Data;
using Amusing.Helpers;
using MySqlConnector;

namespace Amusing.Services;

public sealed class RegistrationPlanningExportService(IConfiguration config, UserContextHelper users)
{
    public async Task<PlanningExportData> ReadAsync(uint festival, CancellationToken ct = default)
    {
        await using var connection = new MySqlConnection(config.GetConnectionString("DefaultConnection"));
        await connection.OpenAsync(ct);
        await using var tx = await connection.BeginTransactionAsync(IsolationLevel.RepeatableRead, ct);
        var actor = await users.GetUserIdAsync();
        await using (var auth = new MySqlCommand("SELECT COUNT(*) FROM ah_beheer WHERE user_id=@actor AND NULLIF(TRIM(role),'') IS NOT NULL", connection, tx))
        {
            auth.Parameters.AddWithValue("@actor", actor);
            if (actor <= 0 || Convert.ToInt32(await auth.ExecuteScalarAsync(ct)) != 1) throw new UnauthorizedAccessException();
        }
        int year;
        await using (var edition = Command("SELECT YEAR(festivaldatum) FROM ah_festivals WHERE festival_id=@festival"))
        {
            var value = await edition.ExecuteScalarAsync(ct);
            if (value is null or DBNull) throw new InvalidOperationException("De festivaleditie bestaat niet.");
            year = Convert.ToInt32(value);
        }
        var rows = new List<PlanningRegistration>();
        await using (var cmd = Command("""
            SELECT i.zanggroep_id,COALESCE(g.naam,''),COALESCE(g.standplaats,''),COALESCE(i.podiumsoort,''),
                COALESCE(i.aantal_deelnemers,0),COALESCE(gen.nl,''),
                CASE WHEN CAST(i.ingeschreven AS CHAR) LIKE '0000-%' THEN NULL ELSE i.ingeschreven END,
                CASE WHEN CAST(i.betaald AS CHAR) LIKE '0000-%' THEN NULL ELSE i.betaald END,
                i.betaald_bedrag,
                CASE WHEN CAST(i.bevestigd AS CHAR) LIKE '0000-%' THEN NULL ELSE i.bevestigd END,
                LOWER(COALESCE(i.wens_3,''))='ja',COALESCE(i.binnenoptredens,0),COALESCE(i.buitenoptredens,0),
                COALESCE(i.wachtlijst,0),i.afgehaakt IS NOT NULL AND CAST(i.afgehaakt AS CHAR) NOT LIKE '0000-%',
                NOT EXISTS(SELECT 1 FROM ah_inschrijvingen old JOIN ah_festivals earlier ON earlier.festival_id=old.festival_id
                    WHERE old.zanggroep_id=i.zanggroep_id AND earlier.festivaldatum<f.festivaldatum)
            FROM ah_inschrijvingen i JOIN ah_festivals f ON f.festival_id=i.festival_id
            LEFT JOIN ah_zanggroepen g ON g.zanggroep_id=i.zanggroep_id
            LEFT JOIN ah_genres gen ON gen.genre_id=g.genre_id
            WHERE i.festival_id=@festival
            """))
        await using (var r = await cmd.ExecuteReaderAsync(ct))
        {
            while (await r.ReadAsync(ct))
            {
                DateTime? Date(int col) => r.IsDBNull(col) ? null : r.GetDateTime(col);
                rows.Add(new PlanningRegistration
                {
                    GroupId = r.GetUInt32(0),
                    Name = r.GetString(1),
                    City = r.GetString(2),
                    Stage = r.GetString(3),
                    Singers = r.GetInt32(4),
                    Genre = r.GetString(5),
                    RegisteredAt = Date(6),
                    PaidOn = Date(7),
                    PaidAmount = r.IsDBNull(8) ? null : r.GetDecimal(8),
                    ConfirmedOn = Date(9),
                    Battle = r.GetBoolean(10),
                    Inside = r.GetInt32(11),
                    Outside = r.GetInt32(12),
                    Waitlisted = r.GetBoolean(13),
                    Cancelled = r.GetBoolean(14),
                    IsNew = r.GetBoolean(15)
                });
            }
        }
        var contacts = new Dictionary<uint, (string Name, string Email, string Mobile)>();
        var conductors = new Dictionary<uint, List<string>>();
        await using (var cmd = Command("""
            SELECT roles.zanggroep_id,roles.rol,
                CONCAT_WS(' ',NULLIF(TRIM(p.voornaam),''),NULLIF(TRIM(p.tussenvoegsel),''),NULLIF(TRIM(p.achternaam),'')),
                COALESCE(p.email,''),COALESCE(c.telefoon_mobiel,'')
            FROM ah_personen_rollen roles JOIN ah_personen p ON p.persoon_id=roles.persoon_id AND p.actief=1
            LEFT JOIN ah_contactgegevens c ON c.persoon_id=p.persoon_id
            WHERE roles.rol IN ('contactpersoon1','dirigent') AND EXISTS(
                SELECT 1 FROM ah_inschrijvingen i WHERE i.festival_id=@festival AND i.zanggroep_id=roles.zanggroep_id)
            ORDER BY roles.zanggroep_id,p.persoon_id
            """))
        await using (var r = await cmd.ExecuteReaderAsync(ct))
        {
            while (await r.ReadAsync(ct))
            {
                var id = r.GetUInt32(0);
                if (r.GetString(1) == "contactpersoon1") contacts.TryAdd(id, (r.GetString(2), r.GetString(3), r.GetString(4)));
                else
                {
                    if (!conductors.TryGetValue(id, out var names)) conductors[id] = names = [];
                    names.Add(r.GetString(2));
                }
            }
        }
        await tx.CommitAsync(ct);
        return new(year, rows.Select(row =>
        {
            contacts.TryGetValue(row.GroupId, out var contact);
            return row with { Contact = contact.Name ?? "", Email = contact.Email ?? "", Mobile = contact.Mobile ?? "", Conductors = conductors.GetValueOrDefault(row.GroupId) ?? [] };
        }).ToArray());

        MySqlCommand Command(string sql)
        {
            var cmd = new MySqlCommand(sql, connection, tx);
            cmd.Parameters.AddWithValue("@festival", festival);
            return cmd;
        }
    }
}
