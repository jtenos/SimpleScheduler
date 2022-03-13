namespace SimpleSchedulerService;

public class Worker
    : BackgroundService
{
    private readonly JobScheduler _scheduler;

    public Worker(JobScheduler scheduler)
        => _scheduler = scheduler;

    protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.CompletedTask;

    public override async Task StartAsync(CancellationToken cancellationToken)
        => await _scheduler.StartSchedulerAsync(cancellationToken).ConfigureAwait(false);

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _scheduler.Dispose();
        return Task.CompletedTask;
    }
}
