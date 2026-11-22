using Amusing.Services.MailQueue;
using MySqlConnector;
using MimeKit;
using Xunit;

namespace Beheer.Tests;

public sealed class QueueDatabaseFactAttribute : FactAttribute
{
    public QueueDatabaseFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("BEHEER_MAIL_TEST_CONNECTION")))
            Skip="Set BEHEER_MAIL_TEST_CONNECTION to an isolated mijn_test_ database.";
    }
}
public class MailQueueStoreTests : IAsyncLifetime
{
    private string _connection="";
    private readonly TestClock _clock=new();
    private MailQueueStore Store => new(_connection,_clock,TimeSpan.Zero);
    public async Task InitializeAsync()
    {
        _connection=Environment.GetEnvironmentVariable("BEHEER_MAIL_TEST_CONNECTION")??"";
        if(_connection.Length==0)return;
        var builder=new MySqlConnectionStringBuilder(_connection);
        if(!builder.Database.StartsWith("mijn_test_",StringComparison.Ordinal))throw new InvalidOperationException("Only isolated test schemas allowed.");
        await Sql("DROP TABLE IF EXISTS beheer_mail_message; DROP TABLE IF EXISTS beheer_mail_campaign; DROP TABLE IF EXISTS beheer_mail_gate;");
        await Sql(await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory,"001-mailwachtrij.sql")));
    }
    public Task DisposeAsync()=>Task.CompletedTask;
    private async Task Sql(string sql){await using var db=new MySqlConnection(_connection);await db.OpenAsync();await using var cmd=new MySqlCommand(sql,db);await cmd.ExecuteNonQueryAsync();}
    private static QueueMessage Message(string label="one")
    {
        var mime=new MimeMessage();mime.From.Add(MailboxAddress.Parse("sender@example.invalid"));mime.To.Add(MailboxAddress.Parse("contact@example.invalid"));
        mime.MessageId=MimeKit.Utils.MimeUtils.GenerateMessageId();mime.Subject=label;mime.Body=new TextPart("html"){Text="<p>"+label+"</p>"};
        using var stream=new MemoryStream();mime.WriteTo(stream);return new("contact@example.invalid",stream.ToArray());
    }
    [QueueDatabaseFact]public async Task EnqueueIsIdempotentAndKeepsDifferentCopiesAtSameAddress()
    {
        var id=Guid.NewGuid();QueueMessage[] messages=[Message("koor1"),Message("koor2")];
        await Store.EnqueueAsync(id,22,"Test",1,messages);await Store.EnqueueAsync(id,22,"Test",1,messages);
        Assert.Equal(2,(await Store.MessagesAsync(id.ToString("N"))).Count);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>Store.EnqueueAsync(id,22,"Test",1,[Message("changed")]));
        Assert.Equal(2,(await Store.CampaignsAsync()).Single().Total);
    }
    [QueueDatabaseFact]public async Task QuotaBlocksOtherCampaignAndSurvivesStoreRestart()
    {
        var first=Guid.NewGuid();var second=Guid.NewGuid();
        await Store.EnqueueAsync(first,22,"First",1,[Message()]);await Store.EnqueueAsync(second,22,"Second",1,[Message()]);
        var rejected=new FakeTransport(new(MailOutcome.TemporaryFailure,"Exceeding quota",451,true));
        Assert.True(await Store.ProcessNextAsync(rejected));
        var accepted=new FakeTransport(new(MailOutcome.Accepted));
        Assert.False(await Store.ProcessNextAsync(accepted));Assert.Equal(0,accepted.Calls);
        _clock.Advance(TimeSpan.FromHours(1));
        Assert.True(await Store.ProcessNextAsync(accepted));Assert.True(await Store.ProcessNextAsync(accepted));
        Assert.Equal(2,(await Store.CampaignsAsync()).Sum(c=>c.Accepted));
        Assert.Equal(2,(await Store.MessagesAsync(first.ToString("N"))).Single().Attempts);
    }
    [QueueDatabaseFact]public async Task TwoWorkersNeverSubmitSameMessageConcurrently()
    {
        await Store.EnqueueAsync(Guid.NewGuid(),22,"Test",1,[Message()]);
        var entered=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var transport=new FakeTransport(new(MailOutcome.Accepted),async()=>{entered.SetResult();await release.Task;});
        var first=Store.ProcessNextAsync(transport);await entered.Task.WaitAsync(TimeSpan.FromSeconds(10));
        try {Assert.False(await Store.ProcessNextAsync(transport));Assert.Equal(1,transport.Calls);}
        finally{release.TrySetResult();await first;}
    }
    [QueueDatabaseFact]public async Task OrphanedSendingMessageNeedsReviewInsteadOfReplay()
    {
        var id=Guid.NewGuid();await Store.EnqueueAsync(id,22,"Test",1,[Message()]);await Sql("UPDATE beheer_mail_message SET state='sending'");
        var transport=new FakeTransport(new(MailOutcome.Accepted));Assert.False(await Store.ProcessNextAsync(transport));
        Assert.Equal(0,transport.Calls);Assert.Equal(QueueState.Review,(await Store.MessagesAsync(id.ToString("N"))).Single().State);
    }
    [QueueDatabaseFact]public async Task PermanentFailureAndAmbiguousAcceptanceAreNotRetried()
    {
        foreach(var outcome in new[]{MailOutcome.PermanentFailure,MailOutcome.Uncertain})
        {
            var id=Guid.NewGuid();await Store.EnqueueAsync(id,22,"Test",1,[Message()]);var transport=new FakeTransport(new(outcome,"Failure"));
            Assert.True(await Store.ProcessNextAsync(transport));Assert.False(await Store.ProcessNextAsync(transport));Assert.Equal(1,transport.Calls);
            Assert.Equal(outcome==MailOutcome.Uncertain?QueueState.Review:QueueState.Failed,(await Store.MessagesAsync(id.ToString("N"))).Single().State);
        }
    }
    [QueueDatabaseFact]public async Task AuthenticationErrorBlocksRemainingMessages()
    {
        await Store.EnqueueAsync(Guid.NewGuid(),22,"Test",1,[Message("first"),Message("second")]);
        var transport=new FakeTransport(new(MailOutcome.ConfigurationFailure,"SMTP-aanmelding mislukt",535));
        Assert.True(await Store.ProcessNextAsync(transport));_clock.Advance(TimeSpan.FromDays(1));Assert.False(await Store.ProcessNextAsync(transport));Assert.Equal(1,transport.Calls);
    }
    [QueueDatabaseFact]public async Task FiveDayOldWaitingMailNeedsReview()
    {
        var id=Guid.NewGuid();await Store.EnqueueAsync(id,22,"Test",1,[Message()]);_clock.Advance(TimeSpan.FromDays(5));
        var transport=new FakeTransport(new(MailOutcome.Accepted));Assert.True(await Store.ProcessNextAsync(transport));Assert.Equal(0,transport.Calls);
        Assert.Equal(QueueState.Review,(await Store.MessagesAsync(id.ToString("N"))).Single().State);
    }
    [QueueDatabaseFact]public async Task SharedSpacingAppliesToSecondCampaign()
    {
        await Store.EnqueueAsync(Guid.NewGuid(),22,"First",1,[Message()]);await Store.EnqueueAsync(Guid.NewGuid(),22,"Second",1,[Message()]);
        var store=new MailQueueStore(_connection,_clock);var transport=new FakeTransport(new(MailOutcome.Accepted));
        Assert.True(await store.ProcessNextAsync(transport));Assert.False(await store.ProcessNextAsync(transport));
        _clock.Advance(TimeSpan.FromSeconds(19));Assert.True(await store.ProcessNextAsync(transport));
    }
    private sealed class TestClock:TimeProvider
    {
        private DateTimeOffset _now=new(2026,11,22,12,0,0,TimeSpan.Zero);
        public override DateTimeOffset GetUtcNow()=>_now;
        public void Advance(TimeSpan duration)=>_now+=duration;
    }
    private sealed class FakeTransport(MailAttempt result,Func<Task>? wait=null):IQueueMailTransport
    {
        public int Calls;
        public async Task<MailAttempt> SendAsync(byte[] mime,CancellationToken ct){Interlocked.Increment(ref Calls);if(wait is not null)await wait();return result;}
    }
}
