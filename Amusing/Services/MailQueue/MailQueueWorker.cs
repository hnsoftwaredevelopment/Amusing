namespace Amusing.Services.MailQueue;

public sealed class MailQueueWorker(MailQueueStore store, IQueueMailTransport transport,
    ILogger<MailQueueWorker> logger, IConfiguration configuration, IHostEnvironment environment) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if(!MailQueueEnvironment.MaySend(configuration,environment.EnvironmentName)) return;
        using var timer=new PeriodicTimer(TimeSpan.FromSeconds(5));
        try
        {
            do
            {
                try { await store.ProcessNextAsync(transport,stoppingToken); }
                catch(Exception ex) when(!stoppingToken.IsCancellationRequested)
                { logger.LogError("Mailwachtrij kon niet worden verwerkt ({ErrorType}).",ex.GetType().Name); }
            } while(await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch(OperationCanceledException) when(stoppingToken.IsCancellationRequested) { }
    }
}
