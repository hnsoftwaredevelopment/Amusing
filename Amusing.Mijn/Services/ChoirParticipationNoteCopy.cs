using System.Net.Mail;
using System.Text.Json;

namespace Amusing.Mijn.Services;

public sealed partial class ChoirEnrollments
{
    private bool CanSendNoteCopy(List<Role> roles, uint actor) =>
        supportContext?.HttpContext?.User.IsAdministrator() != true &&
        roles.Any(role => role.Person == actor && role.Active && role.Name is "contactpersoon1" or "contactpersoon2" or "penningmeester");

    public async Task<string> QueueNoteCopyAsync(uint actor, uint group, uint festival, CancellationToken ct)
    {
        await using var connection = await source.OpenConnectionAsync(ct);
        await using var tx = await connection.BeginTransactionAsync(ct);
        var roles = await RolesAsync(connection, tx, group, true, ct);
        if (!CanSendNoteCopy(roles, actor)) return "geen-toegang";
        var choir = await GroupAsync(connection, tx, group, true, ct);
        var registration = await RegistrationAsync(connection, tx, festival, group, ct, locked: true);
        if (choir is null || registration is null or { Cancelled: true } or { Waitlisted: true }) return "niet-beschikbaar";

        await using (var pending = Command(connection, tx, """
            SELECT 1 FROM mijn_inschrijfmail WHERE festival_id=@festival AND zanggroep_id=@group
                AND soort='kopienota' AND verzonden IS NULL LIMIT 1
            """))
        {
            Add(pending, "@festival", festival);
            Add(pending, "@group", group);
            if (await pending.ExecuteScalarAsync(ct) is not null) return "al-klaargezet";
        }

        string? recipient = null;
        string language = "nl";
        string? recipientName = null;
        await using (var contacts = Command(connection, tx, """
            SELECT p.email,COALESCE(v.taal,'nl'),CONCAT_WS(' ',NULLIF(TRIM(p.voornaam),''),NULLIF(TRIM(p.tussenvoegsel),''),NULLIF(TRIM(p.achternaam),'')) FROM ah_personen_rollen r
            JOIN ah_personen p USING (persoon_id) LEFT JOIN mijn_taalvoorkeur v USING (persoon_id)
            WHERE r.zanggroep_id=@group AND r.rol='penningmeester' AND p.actief=1 ORDER BY p.persoon_id
            """))
        {
            Add(contacts, "@group", group);
            await using var reader = await contacts.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                if (reader.IsDBNull(0) || !MailAddress.TryCreate(reader.GetString(0).Trim(), out var address)) continue;
                recipient = address.Address;
                language = reader.GetString(1);
                recipientName = reader.GetString(2);
                break;
            }
        }
        if (recipient is null) return "geen-penningmeester";

        await using var edition = Command(connection, tx, "SELECT festivaldatum FROM ah_festivals WHERE festival_id=@festival");
        Add(edition, "@festival", festival);
        if (await edition.ExecuteScalarAsync(ct) is not DateTime date) return "niet-beschikbaar";
        var amount = await ParticipationAmountAsync(connection, festival, registration, ct, tx);
        if (amount is null) return "geen-tarief";

        var note = new ParticipationNote(festival, group, DateOnly.FromDateTime(date), DateOnly.FromDateTime(clock.Now),
            choir.Name, registration.Headcount, registration.PodiumType, amount, registration.Paid) { IsCopy = true, MailLanguage = language, RecipientName = recipientName };
        note = await EnrichNoteAsync(connection,tx,note,language,ct);
        var (subject, body, html) = await CreateNoteMailAsync(connection,tx,note,language,choir.Name.Replace('\r',' ').Replace('\n',' '),ct);
        await using var queue = Command(connection, tx, """
            INSERT INTO mijn_inschrijfmail(festival_id,zanggroep_id,soort,ontvanger,onderwerp,bericht,volgende_poging,
                aanvrager_id,aanvrager_type,nota_gegevens,ontvanger_rol,opmaak_html)
            VALUES(@festival,@group,'kopienota',@recipient,@subject,@body,UTC_TIMESTAMP(6),@actor,'choir_person',@note,'penningmeester',@html)
            ON DUPLICATE KEY UPDATE onderwerp=VALUES(onderwerp),bericht=VALUES(bericht),volgende_poging=VALUES(volgende_poging),
                aanvrager_id=VALUES(aanvrager_id),nota_gegevens=VALUES(nota_gegevens),opmaak_html=VALUES(opmaak_html),pogingen=0,verzonden=NULL
            """);
        Add(queue, "@festival", festival);
        Add(queue, "@group", group);
        Add(queue, "@recipient", recipient);
        Add(queue, "@subject", subject);
        Add(queue, "@body", body);
        Add(queue, "@actor", actor);
        Add(queue, "@note", JsonSerializer.Serialize(note));
        Add(queue, "@html", (object?)html ?? DBNull.Value);
        await queue.ExecuteNonQueryAsync(ct);
        await tx.CommitAsync(ct);
        return "klaargezet";
    }
}
