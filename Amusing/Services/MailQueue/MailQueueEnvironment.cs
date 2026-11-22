namespace Amusing.Services.MailQueue;

public static class MailQueueEnvironment
{
    public static bool MaySend(IConfiguration configuration,string hostEnvironment) =>
        string.Equals(hostEnvironment,"Production",StringComparison.OrdinalIgnoreCase) &&
        configuration.GetValue<bool>("MailQueue:Enabled") && configuration.GetValue<bool>("MailQueue:SendEnabled") &&
        string.Equals(configuration["Amusing:EnvironmentName"]??hostEnvironment,"Production",StringComparison.OrdinalIgnoreCase);
}
