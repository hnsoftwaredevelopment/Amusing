using System.Security.Cryptography;
using System.Text;
using MySqlConnector;
using MimeKit;

namespace Amusing.Services.MailQueue;

/// <summary>Durable bulk outbox. A non-pooled advisory-lock connection serializes all workers in this database.</summary>
public sealed class MailQueueStore(string connectionString, TimeProvider? clock = null, TimeSpan? minimumSpacing = null)
{
    private readonly TimeProvider _clock = clock ?? TimeProvider.System;
    private readonly TimeSpan _spacing = minimumSpacing ?? TimeSpan.FromSeconds(19);
    private string ConnectionString => new MySqlConnectionStringBuilder(connectionString) { Pooling = false }.ConnectionString;
    private DateTime Now => _clock.GetUtcNow().UtcDateTime;

    public async Task<string> EnqueueAsync(Guid campaign, uint templateId, string templateName, uint actorId,
        IReadOnlyList<QueueMessage> messages, CancellationToken ct = default)
    {
        if (messages.Count == 0) throw new ArgumentException("Geen ontvangers.", nameof(messages));
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        void Hash(byte[] data) { hash.AppendData(BitConverter.GetBytes(data.Length)); hash.AppendData(data); }
        Hash(Encoding.UTF8.GetBytes($"{templateId}:{templateName}:{actorId}"));
        foreach (var item in messages)
        {
            using var stream = new MemoryStream(item.Mime, false);
            var mime = await MimeMessage.LoadAsync(stream, ct);
            if (mime.To.Mailboxes.Count() != 1 || mime.Cc.Count != 0 || mime.Bcc.Count != 0 ||
                !string.Equals(mime.To.Mailboxes.Single().Address, item.Recipient, StringComparison.OrdinalIgnoreCase) ||
                string.IsNullOrWhiteSpace(mime.MessageId))
                throw new ArgumentException("Een wachtrijbericht moet één overeenkomende ontvanger en een Message-ID bevatten.");
            Hash(Encoding.UTF8.GetBytes(item.Recipient)); Hash(item.Mime);
        }
        var requestHash = hash.GetHashAndReset();
        string id = campaign.ToString("N");
        await using var db = new MySqlConnection(ConnectionString); await db.OpenAsync(ct);
        await using var tx = await db.BeginTransactionAsync(ct);
        await using (var insert = Command(db, """
            INSERT INTO beheer_mail_campaign(id,request_hash,template_id,template_name,created_by,created_at)
            VALUES(@id,@hash,@template,@name,@actor,@now) ON DUPLICATE KEY UPDATE id=id
            """, tx, ("id",id),("hash",requestHash),("template",templateId),("name",templateName),("actor",actorId),("now",Now)))
            await insert.ExecuteNonQueryAsync(ct);
        await using (var verify = Command(db,"SELECT request_hash FROM beheer_mail_campaign WHERE id=@id FOR UPDATE",tx,("id",id)))
        {
            var stored = (byte[])(await verify.ExecuteScalarAsync(ct))!;
            if (!CryptographicOperations.FixedTimeEquals(stored, requestHash)) throw new InvalidOperationException("Campagne-id bestaat met andere inhoud.");
        }
        for (int i=0;i<messages.Count;i++)
        {
            var item=messages[i];
            await using var insert=Command(db,"""
                INSERT INTO beheer_mail_message(campaign_id,ordinal_no,recipient,mime,created_at,next_attempt)
                VALUES(@id,@ordinal,@recipient,@mime,@now,@now) ON DUPLICATE KEY UPDATE ordinal_no=ordinal_no
                """,tx,("id",id),("ordinal",i),("recipient",item.Recipient),("mime",item.Mime),("now",Now));
            await insert.ExecuteNonQueryAsync(ct);
        }
        await tx.CommitAsync(ct); return id;
    }

    public async Task<bool> ProcessNextAsync(IQueueMailTransport transport, CancellationToken ct=default)
    {
        await using var db=new MySqlConnection(ConnectionString); await db.OpenAsync(ct);
        string lockName="beheer-mail:"+Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(db.Database)))[..40];
        await using (var acquire=Command(db,"SELECT GET_LOCK(@key,0)",null,("key",lockName)))
            if(Convert.ToInt32(await acquire.ExecuteScalarAsync(ct))!=1) return false;
        try
        {
            // Holding the same server lock proves no previous owner is still operating with a valid claim.
            // A crash after SMTP acceptance cannot safely be replayed; quarantine instead.
            await Execute(db,"""
                UPDATE beheer_mail_message SET state='review',next_attempt=NULL,
                last_error='Verwerking onderbroken; SMTP-acceptatie onbekend. Controleer voor opnieuw verzenden.'
                WHERE state='sending'
                """,ct);
            DateTime now=Now;
            await using(var gate=Command(db,"SELECT last_attempt,paused_until,blocked_reason FROM beheer_mail_gate WHERE id=1"))
            await using(var reader=await gate.ExecuteReaderAsync(ct))
            {
                if(!await reader.ReadAsync(ct)) throw new InvalidOperationException("Wachtrijmigratie ontbreekt.");
                if(!reader.IsDBNull(2) || (!reader.IsDBNull(1)&&reader.GetDateTime(1)>now) ||
                    (!reader.IsDBNull(0)&&reader.GetDateTime(0).Add(_spacing)>now)) return false;
            }
            ulong messageId;byte[] mime;int attempts;DateTime created;
            await using(var select=Command(db,"""
                SELECT id,mime,attempts,created_at FROM beheer_mail_message
                WHERE state='waiting' AND next_attempt<=@now ORDER BY next_attempt,id LIMIT 1
                """,null,("now",now)))
            await using(var reader=await select.ExecuteReaderAsync(ct))
            {
                if(!await reader.ReadAsync(ct)) return false;
                messageId=reader.GetUInt64(0);mime=(byte[])reader.GetValue(1);attempts=reader.GetInt32(2)+1;created=reader.GetDateTime(3);
            }
            if(now-created>=TimeSpan.FromDays(5))
            {
                await Execute(db,"UPDATE beheer_mail_message SET state='review',next_attempt=NULL,last_error='Verzending wacht langer dan vijf dagen.' WHERE id=@id",ct,("id",messageId));
                return true;
            }
            await Execute(db,"UPDATE beheer_mail_message SET state='sending',attempts=attempts+1,last_attempt=@now WHERE id=@id",ct,("id",messageId),("now",now));
            await Execute(db,"UPDATE beheer_mail_gate SET last_attempt=@now WHERE id=1",ct,("now",now));
            MailAttempt result;
            try { result=await transport.SendAsync(mime,ct); }
            catch(Exception) { result=new(MailOutcome.Uncertain,"Verzending onderbroken; SMTP-uitkomst onbekend."); }
            now=Now;
            var decision=MailRetryPolicy.Decide(result,created,now,attempts);
            // Complete despite a browser disconnect or host cancellation. A DB failure leaves 'sending' for quarantine.
            await using var tx=await db.BeginTransactionAsync(CancellationToken.None);
            await using(var update=Command(db,"""
                UPDATE beheer_mail_message SET state=@state,next_attempt=@next,last_error=@error,smtp_code=@code,
                accepted_at=@accepted WHERE id=@id AND state='sending'
                """,tx,("state",decision.State),("next",decision.NextAttempt),("error",result.Error),("code",result.SmtpCode),
                ("accepted",decision.State==QueueState.Accepted ? now : null),("id",messageId)))
                await update.ExecuteNonQueryAsync(CancellationToken.None);
            if(decision.PauseQueue || decision.BlockQueue)
            {
                await using var pause=Command(db,"UPDATE beheer_mail_gate SET paused_until=@until,blocked_reason=@reason WHERE id=1",tx,
                    ("until",decision.PauseQueue?decision.NextAttempt:null),("reason",decision.BlockQueue?result.Error:null));
                await pause.ExecuteNonQueryAsync(CancellationToken.None);
            }
            await tx.CommitAsync(CancellationToken.None);return true;
        }
        finally
        {
            try { await using var release=Command(db,"DO RELEASE_LOCK(@key)",null,("key",lockName));await release.ExecuteNonQueryAsync(CancellationToken.None); }
            catch { /* Non-pooled connection is disposed; its lock cannot return to the pool. */ }
        }
    }

    public async Task<IReadOnlyList<CampaignProgress>> CampaignsAsync(CancellationToken ct=default)
    {
        await using var db=new MySqlConnection(ConnectionString);await db.OpenAsync(ct);
        await using var cmd=Command(db,"""
            SELECT c.id,c.template_name,c.created_at,COUNT(*),SUM(m.state='accepted'),SUM(m.state IN ('waiting','sending')),
                SUM(m.state='failed'),SUM(m.state='review') FROM beheer_mail_campaign c
            JOIN beheer_mail_message m ON m.campaign_id=c.id GROUP BY c.id,c.template_name,c.created_at
            ORDER BY c.created_at DESC LIMIT 100
            """);
        await using var reader=await cmd.ExecuteReaderAsync(ct);var rows=new List<CampaignProgress>();
        while(await reader.ReadAsync(ct)) rows.Add(new(reader.GetString(0),reader.GetString(1),reader.GetDateTime(2),reader.GetInt32(3),reader.GetInt32(4),reader.GetInt32(5),reader.GetInt32(6),reader.GetInt32(7)));
        return rows;
    }
    public async Task<IReadOnlyList<QueueMessageProgress>> MessagesAsync(string campaign,CancellationToken ct=default)
    {
        await using var db=new MySqlConnection(ConnectionString);await db.OpenAsync(ct);
        await using var cmd=Command(db,"SELECT recipient,state,attempts,next_attempt,last_error FROM beheer_mail_message WHERE campaign_id=@id ORDER BY ordinal_no",null,("id",campaign));
        await using var reader=await cmd.ExecuteReaderAsync(ct);var rows=new List<QueueMessageProgress>();
        while(await reader.ReadAsync(ct))rows.Add(new(reader.GetString(0),reader.GetString(1),reader.GetInt32(2),reader.IsDBNull(3)?null:reader.GetDateTime(3),reader.IsDBNull(4)?null:reader.GetString(4)));
        return rows;
    }
    private static MySqlCommand Command(MySqlConnection db,string sql,MySqlTransaction? tx=null,params (string Key,object? Value)[] parameters)
    {
        var cmd=new MySqlCommand(sql,db,tx);foreach(var (key,value) in parameters)cmd.Parameters.AddWithValue("@"+key,value??DBNull.Value);return cmd;
    }
    private static async Task Execute(MySqlConnection db,string sql,CancellationToken ct,params (string Key,object? Value)[] parameters)
    { await using var cmd=Command(db,sql,null,parameters);await cmd.ExecuteNonQueryAsync(ct); }
}
