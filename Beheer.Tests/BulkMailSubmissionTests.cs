using Amusing.Services.MailQueue;
using Xunit;

namespace Beheer.Tests;

public class BulkMailSubmissionTests
{
    private static BulkMailDraft Draft(string name="Original")=>new(Guid.NewGuid(),22,name,[new("test@example.invalid",[1])]);

    [Fact] public async Task UncertainCommitRetriesTheExactOriginalSnapshot()
    {
        var submission=new BulkMailSubmission();var original=Draft();var calls=0;
        Task<string> Enqueue(BulkMailDraft draft,uint actor)
        {
            Assert.Same(original,draft);Assert.Equal(7u,actor);
            if(++calls==1)throw new IOException("Reply lost after commit");
            return Task.FromResult(draft.Id.ToString("N"));
        }
        await Assert.ThrowsAsync<IOException>(()=>submission.SubmitAsync(original,7,Enqueue));
        Assert.True(submission.IsPending);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>submission.SubmitAsync(Draft("Changed recipients"),7,Enqueue));
        Assert.Equal(1,calls);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>submission.SubmitAsync(null,8,Enqueue));
        await submission.SubmitAsync(null,7,Enqueue);
        Assert.Equal(original.Id.ToString("N"),submission.CampaignId);
        Assert.Equal(2,calls);Assert.False(submission.IsPending);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>submission.SubmitAsync(null,7,Enqueue));
        Assert.Equal(2,calls);
    }

    [Fact] public async Task TemplateResetAndConcurrentSubmitCannotChangeAnActiveRequest()
    {
        var submission=new BulkMailSubmission();var original=Draft();
        var started=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var finish=new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        var sending=submission.SubmitAsync(original,7,(_,_)=>{started.SetResult();return finish.Task;});
        await started.Task;
        try
        {
            Assert.Throws<InvalidOperationException>(submission.Reset);
            await Assert.ThrowsAsync<InvalidOperationException>(()=>submission.SubmitAsync(null,7,(_,_)=>throw new Exception("Must not enqueue twice")));
        }
        finally{finish.SetResult(original.Id.ToString("N"));await sending;}
        Assert.Equal(original.Id.ToString("N"),submission.CampaignId);
        submission.Reset();Assert.Null(submission.CampaignId);
    }

    [Fact] public async Task EmptyRecipientsDoNotLeaveAnUnusablePendingRequest()
    {
        var submission=new BulkMailSubmission();
        await Assert.ThrowsAsync<InvalidOperationException>(()=>submission.SubmitAsync(new(Guid.NewGuid(),22,"Empty",[]),7,(_,_)=>throw new Exception("Must not enqueue")));
        Assert.False(submission.IsPending);Assert.Null(submission.Draft);
        var draft=Draft();await submission.SubmitAsync(draft,7,(d,_)=>Task.FromResult(d.Id.ToString("N")));
        Assert.Equal(draft.Id.ToString("N"),submission.CampaignId);
    }
}
