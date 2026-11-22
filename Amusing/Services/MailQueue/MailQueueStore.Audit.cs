using System.Text;
using MySqlConnector;

namespace Amusing.Services.MailQueue;

public sealed partial class MailQueueStore
{
    private async Task LogFinishedCampaignsAsync(MySqlConnection db,CancellationToken ct)
    {
        var candidates=new List<string>();
        await using(var query=Command(db,"""
            SELECT c.id FROM beheer_mail_campaign c WHERE c.finished_logged_at IS NULL
            AND EXISTS(SELECT 1 FROM beheer_mail_message m WHERE m.campaign_id=c.id)
            AND NOT EXISTS(SELECT 1 FROM beheer_mail_message m WHERE m.campaign_id=c.id AND m.state IN ('waiting','sending'))
            ORDER BY c.created_at,c.id LIMIT 25
            """))
        await using(var reader=await query.ExecuteReaderAsync(ct))
            while(await reader.ReadAsync(ct))candidates.Add(reader.GetString(0));
        foreach(string campaign in candidates)
        {
            await using var tx=await db.BeginTransactionAsync(ct);
            string name;uint template,actor;
            await using(var query=Command(db,"SELECT template_name,template_id,created_by,finished_logged_at FROM beheer_mail_campaign WHERE id=@id FOR UPDATE",tx,("id",campaign)))
            await using(var reader=await query.ExecuteReaderAsync(ct))
            {
                if(!await reader.ReadAsync(ct)||!reader.IsDBNull(3))continue;
                name=reader.GetString(0);template=reader.GetUInt32(1);actor=reader.GetUInt32(2);
            }
            int total,accepted,failed,review,waiting;
            await using(var query=Command(db,"""
                SELECT COUNT(*),COALESCE(SUM(state='accepted'),0),COALESCE(SUM(state='failed'),0),
                COALESCE(SUM(state='review'),0),COALESCE(SUM(state IN ('waiting','sending')),0)
                FROM beheer_mail_message WHERE campaign_id=@id
                """,tx,("id",campaign)))
            await using(var reader=await query.ExecuteReaderAsync(ct))
            {
                await reader.ReadAsync(ct);total=reader.GetInt32(0);accepted=reader.GetInt32(1);failed=reader.GetInt32(2);review=reader.GetInt32(3);waiting=reader.GetInt32(4);
            }
            if(total==0||waiting!=0)continue;
            var report=new StringBuilder($"Automatische afronding van mailing {name}. Verzonden: {accepted}, mislukt: {failed}, totaal: {total}. Controle nodig: {review}.\nCampagne: {campaign}.\nVerzonden betekent geaccepteerd door de mailserver; inboxontvangst is niet bevestigd.");
            if(failed+review>0)
            {
                report.Append("\nNiet verzonden of te controleren:");
                await using var query=Command(db,"SELECT recipient,state,last_error FROM beheer_mail_message WHERE campaign_id=@id AND state IN ('failed','review') ORDER BY ordinal_no",tx,("id",campaign));
                await using var reader=await query.ExecuteReaderAsync(ct);
                while(await reader.ReadAsync(ct))
                {
                    string reason=reader.IsDBNull(2)?"":reader.GetString(2);
                    string line=$"\n- {reader.GetString(0)} ({(reader.GetString(1)==QueueState.Review?"controle nodig":"mislukt")}): {reason[..Math.Min(200,reason.Length)]}";
                    if(report.Length+line.Length>12000){report.Append("\nMeer meldingen staan in de campagne bij Verzendwachtrij.");break;}
                    report.Append(line);
                }
            }
            await using(var insert=Command(db,"""
                INSERT INTO user_log(date,user_id,template_id,area,action,status,report)
                VALUES(@now,@actor,@template,'Mailing','Wachtrij afgerond',@status,@report)
                """,tx,("now",Now),("actor",actor),("template",template),
                ("status",failed+review==0?"success":"unsuccessful"),("report",report.ToString())))
                await insert.ExecuteNonQueryAsync(ct);
            await using(var update=Command(db,"UPDATE beheer_mail_campaign SET finished_logged_at=@now WHERE id=@id",tx,("now",Now),("id",campaign)))
                await update.ExecuteNonQueryAsync(ct);
            await tx.CommitAsync(ct);
        }
    }

    public async Task<QueueGateProgress> GateAsync(CancellationToken ct=default)
    {
        await using var db=new MySqlConnection(ConnectionString);await db.OpenAsync(ct);
        await using var query=Command(db,"SELECT paused_until,blocked_reason FROM beheer_mail_gate WHERE id=1");
        await using var reader=await query.ExecuteReaderAsync(ct);
        if(!await reader.ReadAsync(ct))throw new InvalidOperationException("Wachtrijconfiguratie ontbreekt.");
        return new(reader.IsDBNull(0)?null:reader.GetDateTime(0),reader.IsDBNull(1)?null:reader.GetString(1));
    }
}

public sealed record QueueGateProgress(DateTime? PausedUntil,string? BlockedReason);
