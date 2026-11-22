namespace Amusing.Services.MailQueue;

public static class QueueState
{
    public const string Waiting = "waiting", Sending = "sending", Accepted = "accepted", Failed = "failed", Review = "review";
}
public enum MailOutcome { Accepted, TemporaryFailure, PermanentFailure, ConfigurationFailure, Uncertain }
public sealed record MailAttempt(MailOutcome Outcome, string? Error = null, int? SmtpCode = null, bool Quota = false);
public sealed record RetryDecision(string State, DateTime? NextAttempt = null, bool PauseQueue = false, bool BlockQueue = false);
public sealed record QueueMessage(string Recipient, byte[] Mime, string? ValidationError = null);
public sealed record CampaignProgress(string Id, string Name, DateTime CreatedAt, int Total, int Accepted, int Waiting, int Failed, int Review);
public sealed record QueueMessageProgress(string Recipient, string State, int Attempts, DateTime? NextAttempt, string? Error);

public static class MailRetryPolicy
{
    public static RetryDecision Decide(MailAttempt result, DateTime created, DateTime now, int attempts)
    {
        if (result.Outcome == MailOutcome.Accepted) return new(QueueState.Accepted);
        if (result.Outcome == MailOutcome.PermanentFailure) return new(QueueState.Failed);
        if (result.Outcome == MailOutcome.ConfigurationFailure) return new(QueueState.Review, BlockQueue: true);
        if (result.Outcome == MailOutcome.Uncertain || now - created >= TimeSpan.FromDays(5)) return new(QueueState.Review);
        var delay = result.Quota ? TimeSpan.FromHours(1) : TimeSpan.FromMinutes(Math.Min(60, 5 * Math.Pow(2, Math.Clamp(attempts - 1, 0, 5))));
        return new(QueueState.Waiting, now.Add(delay), PauseQueue: result.Quota);
    }
}

public interface IQueueMailTransport
{
    Task<MailAttempt> SendAsync(byte[] mime, CancellationToken cancellationToken);
}
