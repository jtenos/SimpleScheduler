namespace SimpleSchedulerService;

public sealed class JobScheduler
    : IDisposable
{
    private readonly ILogger<JobScheduler> _logger;
    private readonly IConfiguration _config;
    private readonly JobExecutor _jobExecutor;

    private readonly System.Timers.Timer _timer = new(5000);

    public JobScheduler(
        ILogger<JobScheduler> logger,
        IConfiguration config, 
        JobExecutor jobExecutor
        )
    {
        _logger = logger;
        _config = config;
        _jobExecutor = jobExecutor;
    }

    public async Task StartSchedulerAsync(CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(_config["WorkerPath"]);
        _logger.LogInformation("WorkerPath={workerPath}", _config["WorkerPath"]);
        _logger.LogInformation("ApiUrl={apiUrl}", _config["ApiUrl"]);

        await _jobExecutor.RestartStuckAppsAsync();

        _timer.Elapsed += async (sender, e) =>
        {
            _timer.Stop();
            await _jobExecutor.GoAsync();
            _timer.Start();
        };
        _timer.Start();
    }

    public void Dispose() => _timer.Dispose();
}
