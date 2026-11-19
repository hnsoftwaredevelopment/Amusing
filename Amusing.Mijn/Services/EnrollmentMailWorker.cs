using System.Net;
using System.Net.Mail;
using System.Text.Json;
using MySqlConnector;

namespace Amusing.Mijn.Services;

public sealed class EnrollmentMailWorker(MySqlDataSource source, IConfiguration configuration,
    ILogger<EnrollmentMailWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!configuration.GetValue<bool>("EnrollmentMail:Enabled"))
            return;

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(10));
        do
        {
            try
            {
                await SendNextAsync(stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                logger.LogWarning("De wachtrij voor inschrijfmail kon niet worden verwerkt ({ErrorType}).", ex.GetType().Name);
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task SendNextAsync(CancellationToken ct)
    {
        await using var connection = await source.OpenConnectionAsync(ct);
        await using var acquire = new MySqlCommand(
            "SELECT GET_LOCK(CONCAT(DATABASE(), ':inschrijfmail'), 0)", connection);
        if (Convert.ToInt32(await acquire.ExecuteScalarAsync(ct)) != 1)
            return;

        try
        {
            await using var select = new MySqlCommand("""
                SELECT m.id, m.ontvanger, m.onderwerp, m.bericht, m.aanvrager_id, m.aanvrager_type,
                    m.nota_gegevens, m.ontvanger_rol, m.zanggroep_id, m.festival_id, m.opmaak_html, m.soort
                FROM mijn_inschrijfmail m LEFT JOIN mijn_inschrijfmail prior ON prior.id=m.afhankelijk_van
                WHERE m.verzonden IS NULL AND m.volgende_poging <= UTC_TIMESTAMP(6)
                    AND (m.afhankelijk_van IS NULL OR prior.verzonden IS NOT NULL)
                ORDER BY m.volgende_poging, m.id LIMIT 1
                """, connection);
            ulong id;
            string recipient, subject, body;
            MailActor? initiator = null;
            string? noteJson, recipientRole, html;
            uint group, festival;
            bool copy;
            await using (var reader = await select.ExecuteReaderAsync(ct))
            {
                if (!await reader.ReadAsync(ct))
                    return;
                id = reader.GetUInt64(0);
                recipient = reader.GetString(1);
                subject = reader.GetString(2);
                body = reader.GetString(3);
                if (!reader.IsDBNull(4) && !reader.IsDBNull(5)) initiator = new(reader.GetUInt32(4), reader.GetString(5));
                noteJson = reader.IsDBNull(6) ? null : reader.GetString(6);
                recipientRole = reader.IsDBNull(7) ? null : reader.GetString(7);
                group = reader.GetUInt32(8);
                festival = reader.GetUInt32(9);
                html = reader.IsDBNull(10) ? null : reader.GetString(10);
                copy = reader.GetString(11) == "kopienota";
            }

            await using var retry = new MySqlCommand("""
                UPDATE mijn_inschrijfmail
                SET volgende_poging=TIMESTAMPADD(SECOND, LEAST(3600, 30 * POW(2, LEAST(pogingen, 7))), UTC_TIMESTAMP(6)),
                    pogingen=pogingen+1
                WHERE id=@id
                """, connection);
            retry.Parameters.AddWithValue("@id", id);
            await retry.ExecuteNonQueryAsync(ct);

            try
            {
                var test = configuration.GetValue<bool>("Mijn:IsTestEnvironment");
                var note = noteJson is null ? null : JsonSerializer.Deserialize<ParticipationNote>(noteJson);
                if (note is not null) note = await ChoirEnrollments.EnrichNoteAsync(connection,null,note,note.MailLanguage,ct);
                var pdf = note is null ? null : ParticipationNotePdf.Render(note, test);
                await MijnMail.DeliverAsync(configuration, recipient, subject, body, ct,
                    testRecipient: test && initiator is not null ? await initiator.AddressAsync(source, ct) : null,
                    pdf: pdf, pdfName: note is null ? null : $"deelname-nota-{note.Reference}.pdf", html: html);
            }
            catch (Exception ex) when (!ct.IsCancellationRequested)
            {
                logger.LogWarning("Inschrijfmail {MailId} kon niet worden verzonden ({ErrorType}). De verzending wordt opnieuw geprobeerd.", id, ex.GetType().Name);
                return;
            }

            await using var tx = await connection.BeginTransactionAsync(CancellationToken.None);
            await using var sent = new MySqlCommand(
                """
                UPDATE mijn_inschrijfmail SET verzonden=UTC_TIMESTAMP(6)
                WHERE id=@id AND onderwerp=@subject AND bericht=@body
                    AND nota_gegevens <=> @note AND opmaak_html <=> @html
                """, connection, tx);
            sent.Parameters.AddWithValue("@id", id);
            sent.Parameters.AddWithValue("@subject",subject);
            sent.Parameters.AddWithValue("@body",body);
            sent.Parameters.AddWithValue("@note",(object?)noteJson ?? DBNull.Value);
            sent.Parameters.AddWithValue("@html",(object?)html ?? DBNull.Value);
            await sent.ExecuteNonQueryAsync(CancellationToken.None);
            if (recipientRole is not null)
            {
                await using var audit = new MySqlCommand("""
                    INSERT INTO mijn_audit_log(occurred_at,actor_type,actor_id,action,group_id,festival_id,role_name)
                    VALUES(UTC_TIMESTAMP(6),'system',NULL,@action,@group,@festival,@role)
                    """, connection, tx);
                audit.Parameters.AddWithValue("@group", group);
                audit.Parameters.AddWithValue("@festival", festival);
                audit.Parameters.AddWithValue("@role", recipientRole);
                audit.Parameters.AddWithValue("@action", copy ? "ParticipationNoteCopySent" : "ParticipationNoteSent");
                await audit.ExecuteNonQueryAsync(CancellationToken.None);
            }
            await tx.CommitAsync(CancellationToken.None);
        }
        finally
        {
            await using var release = new MySqlCommand(
                "DO RELEASE_LOCK(CONCAT(DATABASE(), ':inschrijfmail'))", connection);
            await release.ExecuteNonQueryAsync(CancellationToken.None);
        }
    }

}

internal static class MijnMail
{
    internal static async Task DeliverAsync(IConfiguration configuration, string recipient, string subject, string body, CancellationToken ct,
        string? testRecipient = null, bool anonymousAccountMail = false, byte[]? pdf = null, string? pdfName = null,
        string? html = null, bool forceTestPickup = false)
    {
        var from = configuration["EnrollmentMail:From"] ?? "koren@amusing-hengelo.nl";
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeout.CancelAfter(TimeSpan.FromSeconds(20));
        var test = configuration.GetValue<bool>("Mijn:IsTestEnvironment");
        if (test)
        {
            if (anonymousAccountMail) testRecipient = configuration["EnrollmentMail:AnonymousTestRecipient"];
            if (!forceTestPickup)
                recipient = testRecipient ?? (configuration.GetValue<bool>("EnrollmentMail:TestSmtpEnabled") ? "mailopvang@example.invalid" : recipient);
            subject = "[DEV Mijn] " + subject;
        }

        if (test && (forceTestPickup || !configuration.GetValue<bool>("EnrollmentMail:TestSmtpEnabled") ||
            !MailAddress.TryCreate(testRecipient, out var mailbox) ||
            mailbox.Host.EndsWith(".invalid", StringComparison.OrdinalIgnoreCase)))
        {
            using var smtp = new SmtpClient();
            smtp.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
            smtp.PickupDirectoryLocation = configuration["EnrollmentMail:PickupDirectory"]
                ?? throw new InvalidOperationException("Configure the test mail pickup directory.");
            Directory.CreateDirectory(smtp.PickupDirectoryLocation);
            using var pickup = new MailMessage(from, recipient, subject, html is null ? body : "");
            if (html is not null)
            {
                pickup.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(body, System.Text.Encoding.UTF8, "text/plain"));
                pickup.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(html, System.Text.Encoding.UTF8, "text/html"));
            }
            if (pdf is not null) pickup.Attachments.Add(new Attachment(new MemoryStream(pdf), pdfName!, "application/pdf"));
            await smtp.SendMailAsync(pickup, timeout.Token);
            return;
        }

        using var relay = new MailKit.Net.Smtp.SmtpClient
        {
            LocalDomain = configuration["EnrollmentMail:ClientDomain"] ?? "mijn.amusing-hengelo.nl"
        };
        var host = configuration["EnrollmentMail:Host"]
            ?? throw new InvalidOperationException("Configure the SMTP host.");
        var port = configuration.GetValue("EnrollmentMail:Port", 587);
        var security = configuration.GetValue("EnrollmentMail:EnableSsl", true)
            ? MailKit.Security.SecureSocketOptions.StartTls : MailKit.Security.SecureSocketOptions.None;
        await relay.ConnectAsync(host, port, security, timeout.Token);
        if (configuration["EnrollmentMail:Username"] is string username)
            await relay.AuthenticateAsync(username, configuration["EnrollmentMail:Password"], timeout.Token);

        var message = new MimeKit.MimeMessage();
        message.From.Add(MimeKit.MailboxAddress.Parse(from));
        message.To.Add(MimeKit.MailboxAddress.Parse(recipient));
        message.Subject = subject;
        var content = new MimeKit.BodyBuilder { TextBody = body, HtmlBody = html };
        if (pdf is not null) content.Attachments.Add(pdfName!, pdf, new MimeKit.ContentType("application", "pdf"));
        message.Body = content.ToMessageBody();
        await relay.SendAsync(message, timeout.Token);
    }
}
