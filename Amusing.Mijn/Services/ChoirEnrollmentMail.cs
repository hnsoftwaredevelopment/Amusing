using System.Data.Common;
using System.Globalization;
using System.Net.Mail;
using System.Text.Json;

namespace Amusing.Mijn.Services;

public sealed partial class ChoirEnrollments
{
    private async Task QueueEnrollmentMailAsync(DbConnection connection, DbTransaction tx,
        FestivalEdition festival, uint group, string choirName, FestivalRegistration registration, CancellationToken ct, bool noteOnly = false,
        bool placement = false, MailActor? placingActor = null)
    {
        var initiator = placingActor ?? MailActor.From(supportContext?.HttpContext?.User);
        string? primaryContact = null, treasurer = null;
        var confirmationLanguage = "nl";
        var treasurerLanguage = "nl";
        string? primaryContactName = null, treasurerName = null;
        await using (var contacts = Command(connection, tx, """
            SELECT p.email,r.rol,COALESCE(v.taal,'nl'),
                CONCAT_WS(' ',NULLIF(TRIM(p.voornaam),''),NULLIF(TRIM(p.tussenvoegsel),''),NULLIF(TRIM(p.achternaam),''))
            FROM ah_personen_rollen r JOIN ah_personen p ON p.persoon_id=r.persoon_id
            LEFT JOIN mijn_taalvoorkeur v ON v.persoon_id=p.persoon_id
            WHERE r.zanggroep_id=@group AND r.rol IN ('contactpersoon1','penningmeester') AND p.actief=1
            ORDER BY p.persoon_id
            """))
        {
            Add(contacts, "@group", group);
            await using var reader = await contacts.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                if (!reader.IsDBNull(0) && MailAddress.TryCreate(reader.GetString(0).Trim(), out var address))
                {
                    if (reader.GetString(1) == "penningmeester")
                    {
                        if (treasurer is not null) continue;
                        treasurer = address.Address;
                        treasurerLanguage = reader.GetString(2);
                        treasurerName = reader.GetString(3);
                    }
                    else if (primaryContact is null)
                    {
                        primaryContact = address.Address;
                        confirmationLanguage = reader.GetString(2);
                        primaryContactName = reader.GetString(3);
                    }
                }
            }
        }

        var amount = registration.Waitlisted ? null : await ParticipationAmountAsync(connection, festival.Id, registration, ct, tx);
        var subjectName = choirName.Replace('\r', ' ').Replace('\n', ' ');
        var testEnvironment = configuration?.GetValue<bool>("Mijn:IsTestEnvironment") == true;
        var portal = configuration?["AccountRegistration:Origin"] ?? "https://mijn.amusing-hengelo.nl";
        ulong? noteMailId = null;
        var noteRole = treasurer is null ? "contactpersoon1" : "penningmeester";
        var noteRecipient = treasurer ?? primaryContact;
        if (amount is not null && noteRecipient is not null)
        {
            var noteLanguage = treasurer is null ? confirmationLanguage : treasurerLanguage;
            var note = new ParticipationNote(festival.Id, group, festival.Date, DateOnly.FromDateTime(clock.Now),
                choirName, registration.Headcount, registration.PodiumType, amount, registration.Paid)
            {
                MailLanguage = noteLanguage,
                RecipientName = treasurer is null ? primaryContactName : treasurerName
            };
            note = await EnrichNoteAsync(connection,tx,note,noteLanguage,ct);
            var noteMail = await CreateNoteMailAsync(connection,tx,note,noteLanguage,subjectName,ct);
            noteMailId = await QueueAsync("nota", noteRecipient, noteMail.Subject,
                noteMail.Body, JsonSerializer.Serialize(note), noteRole, html: noteMail.Html);
        }
        if (placement && (noteMailId is null || primaryContact is null))
            throw new InvalidOperationException("Voor plaatsing ontbreken een nota of contactpersoon.");
        if (noteOnly) return;
        string[] organisationMailboxes = ["koren", "info"];
        ulong? confirmationId = null;
        if (primaryContact is not null)
        {
            string? subjectTemplate = null, bodyTemplate = null, htmlTemplate = null;
            await using (var layout = Command(connection, tx, """
                SELECT taal,onderwerp,bericht,opmaak_html FROM mijn_mailingsjablonen
                WHERE soort=@type AND taal IN (@language,'nl') AND TRIM(onderwerp)<>'' AND TRIM(bericht)<>''
                ORDER BY taal=@language DESC LIMIT 1
                """))
            {
                Add(layout, "@type", placement ? "plaatsing" : registration.Waitlisted ? "wachtlijst" : "aanmelding");
                Add(layout, "@language", confirmationLanguage);
                await using var reader = await layout.ExecuteReaderAsync(ct);
                if (await reader.ReadAsync(ct))
                {
                    confirmationLanguage = reader.GetString(0);
                    subjectTemplate = reader.GetString(1);
                    bodyTemplate = reader.GetString(2);
                    htmlTemplate = reader.IsDBNull(3) ? null : reader.GetString(3);
                }
                else confirmationLanguage = "nl";
            }
            var confirmation = EnrollmentConfirmationMail.Create(confirmationLanguage, festival.Date, choirName,
                registration, $"{portal.TrimEnd('/')}/koor/{group}/inschrijven", noteMailId is null ? null : noteRole,
                subjectTemplate, bodyTemplate, primaryContactName, htmlTemplate, noteMailId is null ? null : noteRecipient,placement);
            confirmationId = await QueueAsync("koor", primaryContact, confirmation.Subject, confirmation.Body,
                dependsOn: noteMailId, html: confirmation.Html);
        }
        if (placement) return;

        var organisationStatus = registration.Waitlisted
            ? $"Zojuist heeft {choirName} zich aangemeld voor de wachtlijst van het komende festival."
            : $"Zojuist heeft {choirName} zich ingeschreven voor het komende festival.";
        var confirmationStatus = primaryContact is null
            ? "Geen geldig adres voor contactpersoon 1."
            : $"{primaryContact} (contactpersoon 1)";
        var invoiceStatus = noteMailId is not null
            ? $"{noteRecipient} ({(treasurer is null ? "contactpersoon 1, want penningmeester niet ingevuld" : "penningmeester")})"
            : registration.Waitlisted ? "Geen nota bij een wachtlijstinschrijving." : "Geen nota verstuurd.";
        var organisationBody = $"""
            Hallo Amusing-Hengelo organisatie,

            {organisationStatus}
            Aantal deelnemers: {registration.Headcount}
            Podiumtype: {registration.PodiumType}
            Bevestigingsmail is verstuurd naar: {confirmationStatus}
            Nota is verstuurd naar: {invoiceStatus}

            Groeten,
            Korenfestival Amusing Hengelo
            """;
        var organisationSubject = $"Nieuwe inschrijving: {subjectName}";
        string? organisationHtml = null;
        var organisationLayout = await MijnMessageTemplate.ReadAsync(connection,tx,"organisatie","nl",ct);
        if (organisationLayout is not null)
        {
            var fields = new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Groepsnaam"] = choirName, ["Koornaam"] = choirName, ["Editie"] = festival.Date.Year.ToString(CultureInfo.InvariantCulture),
                ["Aantal zangers"] = registration.Headcount.ToString(CultureInfo.InvariantCulture), ["Podiumtype"] = registration.PodiumType,
                ["Inschrijfstatus"] = organisationStatus, ["MailBevestigingOntvanger"] = primaryContact ?? "Geen geldig adres",
                ["RolBevestigingOntvanger"] = "contactpersoon 1", ["MailNotaOntvanger"] = noteMailId is null ? invoiceStatus : noteRecipient!,
                ["RolNotaOntvanger"] = noteMailId is null ? "" : treasurer is null ? "contactpersoon 1, want penningmeester niet ingevuld" : "penningmeester"
            };
            var rendered = organisationLayout.Render(fields);
            organisationSubject = rendered.Subject; organisationBody = rendered.Body; organisationHtml = rendered.Html;
        }
        if (testEnvironment)
        {
            organisationBody = "Testmail. De onderstaande adressen zijn de bedoelde ontvangers. De testberichten zijn omgeleid naar de ingelogde gebruiker.\n\n" + organisationBody;
            if (organisationHtml is not null) organisationHtml = "<p>Testmail. De berichten zijn omgeleid naar de ingelogde gebruiker.</p>" + organisationHtml;
        }
        foreach (var mailbox in testEnvironment ? organisationMailboxes.Take(1) : organisationMailboxes)
            await QueueAsync("organisatie", mailbox + "@amusing-hengelo.nl", organisationSubject,
                organisationBody, dependsOn: confirmationId ?? noteMailId, html: organisationHtml);

        async Task<ulong> QueueAsync(string kind, string recipient, string mailSubject, string body,
            string? note = null, string? role = null, ulong? dependsOn = null, string? html = null)
        {
            await using var cmd = Command(connection, tx, """
                INSERT INTO mijn_inschrijfmail(festival_id,zanggroep_id,soort,ontvanger,onderwerp,bericht,volgende_poging,aanvrager_id,aanvrager_type,nota_gegevens,ontvanger_rol,afhankelijk_van,opmaak_html)
                VALUES(@festival,@group,@kind,@recipient,@subject,@body,UTC_TIMESTAMP(6),@actor,@actorType,@note,@role,@dependsOn,@html)
                ON DUPLICATE KEY UPDATE id=LAST_INSERT_ID(id), onderwerp=VALUES(onderwerp), bericht=VALUES(bericht),
                    volgende_poging=VALUES(volgende_poging), aanvrager_id=VALUES(aanvrager_id),
                    aanvrager_type=VALUES(aanvrager_type), nota_gegevens=VALUES(nota_gegevens),
                    ontvanger_rol=VALUES(ontvanger_rol), afhankelijk_van=VALUES(afhankelijk_van), opmaak_html=VALUES(opmaak_html), pogingen=0, verzonden=NULL;
                SELECT LAST_INSERT_ID();
                """);
            Add(cmd, "@festival", festival.Id);
            Add(cmd, "@group", group);
            Add(cmd, "@kind", kind);
            Add(cmd, "@recipient", recipient);
            Add(cmd, "@subject", testEnvironment && kind != "organisatie" ? $"{mailSubject} ({kind})" : mailSubject);
            Add(cmd, "@body", body);
            Add(cmd, "@actor", (object?)initiator?.Id ?? DBNull.Value);
            Add(cmd, "@actorType", (object?)initiator?.Type ?? DBNull.Value);
            Add(cmd, "@note", (object?)note ?? DBNull.Value);
            Add(cmd, "@role", (object?)role ?? DBNull.Value);
            Add(cmd, "@dependsOn", (object?)dependsOn ?? DBNull.Value);
            Add(cmd, "@html", (object?)html ?? DBNull.Value);
            return Convert.ToUInt64(await cmd.ExecuteScalarAsync(ct));
        }
    }

    private static async Task<(string Subject, string Body, string? Html)> CreateNoteMailAsync(DbConnection connection,
        DbTransaction tx, ParticipationNote note, string language, string subjectName, CancellationToken ct)
    {
        var layout = await MijnMessageTemplate.ReadAsync(connection,tx,note.IsCopy ? "kopienota" : "notamail",language,ct);
        if (layout is not null) return layout.Render(NoteFields(note,layout.Language));
        var fallback = ParticipationNoteMail(note,language,subjectName);
        return (fallback.Subject, fallback.Body + "\n\n" + note.PaymentTerm(language), null);
    }

    private static (string Subject, string Body) ParticipationNoteMail(ParticipationNote note, string language, string subjectName)
    {
        var year = note.FestivalDate.Year;
        var culture = CultureInfo.GetCultureInfo(language switch { "de" => "de-DE", "en" => "en-IE", _ => "nl-NL" });
        var amount = note.Amount.TotaalInclBtw.ToString("C2", culture);
        if (note.IsCopy)
        {
            return language switch
            {
                "de" => ($"Kopie der Teilnahmerechnung Amusing {year} – {subjectName}",
                    $"Hallo,\n\nanbei erhalten Sie eine Kopie der Teilnahmerechnung für {note.ChoirName} für das Festival {year}. Die Gesamtkosten betragen {amount}.\n\nMit freundlichen Grüßen\nKorenfestival Amusing Hengelo"),
                "en" => ($"Copy of the Amusing {year} participation invoice – {subjectName}",
                    $"Hello,\n\nPlease find attached a copy of the participation invoice for {note.ChoirName} for the {year} festival. The total participation fee is {amount}.\n\nKind regards,\nKorenfestival Amusing Hengelo"),
                _ => ($"Kopie deelname-nota Amusing {year} – {subjectName}",
                    $"Hallo,\n\nIn de bijlage staat de kopie van de deelname-nota voor {note.ChoirName} voor het festival van {year}. De totale deelnamekosten bedragen {amount}.\n\nMet vriendelijke groeten,\nKorenfestival Amusing Hengelo")
            };
        }
        return language switch
        {
            "de" => ($"Teilnahmerechnung Amusing {year} – {subjectName}", $"""
                Hallo,

                anbei erhalten Sie die Teilnahmerechnung für {note.ChoirName} für das Korenfestival Amusing Hengelo {year}.
                Die Teilnahme kostet insgesamt {amount}. Die Aufschlüsselung und die Zahlungsdaten finden Sie im beigefügten PDF.

                Mit freundlichen Grüßen
                Korenfestival Amusing Hengelo
                """),
            "en" => ($"Amusing {year} participation invoice – {subjectName}", $"""
                Hello,

                Please find attached the participation invoice for {note.ChoirName} for Korenfestival Amusing Hengelo {year}.
                The total participation fee is {amount}. The attached PDF contains the breakdown and payment details.

                Kind regards,
                Korenfestival Amusing Hengelo
                """),
            _ => ($"Deelname-nota Amusing {year} – {subjectName}", $"""
                Hallo,

                Hierbij ontvangt u de deelname-nota voor {note.ChoirName} voor het Korenfestival Amusing Hengelo {year}.
                De totale deelnamekosten bedragen {amount}. De specificatie en betaalgegevens staan in de PDF in de bijlage.

                Met vriendelijke groeten,
                Korenfestival Amusing Hengelo
                """)
        };
    }
}
