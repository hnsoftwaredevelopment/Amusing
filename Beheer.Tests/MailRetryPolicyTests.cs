using Amusing.Services.MailQueue;
using Xunit;

namespace Beheer.Tests;

public class MailRetryPolicyTests
{
    private static readonly DateTime Now = new(2026, 11, 22, 12, 0, 0, DateTimeKind.Utc);

    [Fact] public void QuotaWaitsAnHourAndPausesOtherCampaigns()
    {
        var result = MailRetryPolicy.Decide(new(MailOutcome.TemporaryFailure, "4.7.0 Exceeding quota", 451, true), Now, Now, 1);
        Assert.Equal(QueueState.Waiting, result.State);
        Assert.Equal(Now.AddHours(1), result.NextAttempt);
        Assert.True(result.PauseQueue);
    }
    [Fact] public void OrdinaryTemporaryFailureUsesBackoff()
    {
        Assert.Equal(Now.AddMinutes(5), MailRetryPolicy.Decide(new(MailOutcome.TemporaryFailure, "Busy", 450), Now, Now, 1).NextAttempt);
        Assert.Equal(Now.AddHours(1), MailRetryPolicy.Decide(new(MailOutcome.TemporaryFailure, "Busy", 450), Now, Now, 8).NextAttempt);
    }
    [Fact] public void DefinitiveRefusalIsNotRetried() => Assert.Equal(QueueState.Failed,
        MailRetryPolicy.Decide(new(MailOutcome.PermanentFailure,"Unknown mailbox",550),Now,Now,1).State);
    [Fact] public void UncertainAcceptanceNeedsReview() => Assert.Equal(QueueState.Review,
        MailRetryPolicy.Decide(new(MailOutcome.Uncertain,"Connection lost"),Now,Now,1).State);
    [Fact] public void AuthenticationErrorBlocksQueue()
    {
        var decision=MailRetryPolicy.Decide(new(MailOutcome.ConfigurationFailure,"Authentication failed",535),Now,Now,1);
        Assert.Equal(QueueState.Review,decision.State); Assert.True(decision.BlockQueue);
    }
    [Fact] public void SuccessRemainsSuccessAfterFiveDays() => Assert.Equal(QueueState.Accepted,
        MailRetryPolicy.Decide(new(MailOutcome.Accepted),Now.AddDays(-6),Now,10).State);
    [Fact] public void OldTemporaryFailureNeedsAttention() => Assert.Equal(QueueState.Review,
        MailRetryPolicy.Decide(new(MailOutcome.TemporaryFailure,"Busy",450),Now.AddDays(-5),Now,10).State);
}
