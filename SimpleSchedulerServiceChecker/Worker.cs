using Timer = System.Timers.Timer;
using System.Text;
using System.ServiceProcess;
using System.Text.Json;
using System.Collections.Immutable;

namespace SimpleSchedulerServiceChecker;

public class Worker
    : BackgroundService
{
    private static readonly Timer _timer = new(TimeSpan.FromMinutes(20).TotalMilliseconds);
    private readonly IConfiguration _config;
    private readonly ILogger<Worker> _logger;

    public Worker(IConfiguration config, ILogger<Worker> logger)
    {
        _config = config;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.CompletedTask;

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        async Task GoAsync()
        {
            _logger.LogInformation("In GoAsync()");
            try
            {
                ImmutableArray<string> serviceNames = _config.GetSection("ServiceNames")
                    .GetChildren()
                    .Select(x => x.Value)
                    .ToImmutableArray();

                _logger.LogInformation("Checking services: {serviceNames}", string.Join(",", serviceNames));

                foreach (var serviceName in serviceNames)
                {
                    if (!IsRunning(serviceName))
                    {
                        _logger.LogCritical("Service {serviceName} is not running", serviceName);
                    }
                }

                await Task.CompletedTask;
                throw new NotImplementedException();
                // TODO: Call API to find overdue jobs, and send in an email
                // https://github.com/dotnet/runtime/issues/43970
                //IServiceScope scope = default!;
                //try
                //{
                //    scope = _scopeFactory.CreateScope();
                //    var jobManager = scope.ServiceProvider.GetRequiredService<IJobManager>();
                //    var overdueJobs = await jobManager.GetOverdueJobsAsync(cancellationToken).ConfigureAwait(false);
                //    if (overdueJobs.Any())
                //    {
                //        var message = new StringBuilder("OVERDUE JOBS<br><br>");
                //        foreach (var job in overdueJobs)
                //        {
                //            message.Append($"{job.Worker.WorkerName}: ");
                //            message.Append($"Queued on {job.Job.QueueDateUTC:yyyy\\-MM\\-dd HH\\:mm\\:ss} (UTC), ");
                //            message.Append($"Status: {job.Job.StatusCode}");
                //            if (job.Job.StatusCode == "ERR")
                //            {
                //                string acknowledgeActionID = job.Job.AcknowledgementID;
                //                string url = _config["ApplicationURL"];
                //                if (!url.EndsWith("/"))
                //                {
                //                    url = $"{url}/";
                //                }
                //                // TODO: This isn't hooked up yet
                //                url += $"ExecuteAction?ActionID={acknowledgeActionID}";
                //                message.Append($" <a href='{url}' target=_blank>Acknowledge</a>");
                //            }
                //            message.Append("<br>");
                //        }
                //        await SendEmailAsync(message.ToString(), cancellationToken);
                //    }
                //}
                //catch
                //{
                //    scope.ServiceProvider.GetRequiredService<DatabaseFactory>().MarkForRollback();
                //    throw;
                //}
                //finally
                //{
                //    await ((IAsyncDisposable)scope).DisposeAsync().ConfigureAwait(false);
                //}
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Error checking scheduler services");
            }
        }
        _timer.Elapsed += async (sender, e) => await GoAsync().ConfigureAwait(false);
        _timer.Enabled = true;
        await GoAsync().ConfigureAwait(false);
    }

    private bool IsRunning(string serviceName)
    {
        try
        {
            // This only runs on Windows, so ignore the platform rule
            #pragma warning disable CA1416 // Validate platform compatibility
            return new ServiceController(serviceName).Status == ServiceControllerStatus.Running;
            #pragma warning restore CA1416 // Validate platform compatibility
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{machineName}: {serviceName} is not running or there is an error",
                Environment.MachineName, serviceName);
            return false;
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
