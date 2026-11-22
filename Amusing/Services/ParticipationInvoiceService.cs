using Amusing.Facturatie;

namespace Amusing.Services;

public interface IParticipationInvoiceData
{
    Task<ParticipationInvoice> LoadAsync(uint groupId, uint festivalId);
}

public interface IParticipationInvoiceSender
{
    Task<string?> SendInvoiceAsync(string recipient, string subject, string html);
}

public class ParticipationInvoiceService(IParticipationInvoiceData data, IParticipationInvoiceSender sender)
{
    public Task<ParticipationInvoice> PreviewAsync(uint groupId, uint festivalId) => data.LoadAsync(groupId, festivalId);

    public async Task<string?> SendAsync(ParticipationInvoice preview)
    {
        var current = await data.LoadAsync(preview.GroupId, preview.FestivalId);
        if (current != preview)
            throw new InvalidOperationException("De inschrijving, het tarief of de ontvanger is gewijzigd. Open de factuur opnieuw en controleer de gegevens.");
        return await sender.SendInvoiceAsync(current.RecipientEmail, current.Subject, current.Html);
    }
}

public class ParticipationInvoiceData(GenericDataService data) : IParticipationInvoiceData
{
    public async Task<ParticipationInvoice> LoadAsync(uint groupId, uint festivalId)
    {
        var parameters = new Dictionary<string, object> { ["@group"] = groupId, ["@festival"] = festivalId };
        var invoices = await data.ExecuteQueryAsync($"""
            SELECT g.naam, YEAR(f.festivaldatum) AS jaar, i.aantal_deelnemers, i.podiumsoort,
                   p.prijs, f.btw_percentage,
                   {FestivalTariefSql.Deelnemersprijs} AS deelnemersprijs,
                   i.afgehaakt IS NOT NULL AS afgehaakt
            FROM amusing.ah_inschrijvingen i
            JOIN amusing.ah_zanggroepen g ON g.zanggroep_id=i.zanggroep_id
            JOIN amusing.ah_festivals f ON f.festival_id=i.festival_id
            {FestivalTariefSql.PodiumtariefJoin}
            WHERE i.zanggroep_id=@group AND i.festival_id=@festival
            """, reader =>
        {
            if (Convert.ToBoolean(reader["afgehaakt"]))
                throw new InvalidOperationException("Deze inschrijving is afgehaakt; hiervoor kan geen deelnamefactuur worden verzonden.");
            if (reader["prijs"] is DBNull)
                throw new InvalidOperationException("Voor het podiumtype van deze festivaleditie ontbreekt een tarief.");
            var singers = Convert.ToInt32(reader["aantal_deelnemers"]);
            if (singers <= 0)
                throw new InvalidOperationException("Vul eerst een geldig deelnemersaantal in bij de inschrijving.");
            if (reader["deelnemersprijs"] is DBNull || reader["btw_percentage"] is DBNull)
                throw new InvalidOperationException("Voor deze festivaleditie ontbreekt een deelnemersstaffel of het btw-tarief.");

            var amount = new Deelnamebedrag(Convert.ToDecimal(reader["deelnemersprijs"]),
                Convert.ToDecimal(reader["prijs"]), Convert.ToDecimal(reader["btw_percentage"]));
            return new ParticipationInvoice(groupId, festivalId, reader["naam"].ToString()!,
                Convert.ToInt32(reader["jaar"]), singers, reader["podiumsoort"].ToString()!, amount, "", "");
        }, parameters);
        if (invoices.Count != 1)
            throw new InvalidOperationException("De festivalinschrijving of het bijbehorende podiumtarief is niet eenduidig gevonden.");

        var contacts = await data.ExecuteQueryAsync("""
            SELECT r.persoon_id, r.rol, COALESCE(p.email,'') AS email
            FROM amusing.ah_personen_rollen r
            JOIN amusing.ah_personen p ON p.persoon_id=r.persoon_id
            WHERE r.zanggroep_id=@group AND p.actief=1 AND r.rol IN ('penningmeester','contactpersoon1')
            """, reader => new InvoiceContact(Convert.ToUInt32(reader["persoon_id"]),
                reader["rol"].ToString()!, reader["email"].ToString()!), parameters);
        var recipient = ParticipationInvoice.SelectRecipient(contacts);
        return invoices[0] with { RecipientEmail = recipient.Email, RecipientRole = recipient.Role };
    }
}
