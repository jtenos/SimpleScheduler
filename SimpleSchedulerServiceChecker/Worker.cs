using Timer = System.Timers.Timer;
using System.ServiceProcess;
using System.Net.Http.Json;
using SimpleSchedulerApiModels.Reply.Jobs;
using SimpleSchedulerApiModels.Request.Jobs;
using SimpleSchedulerApiModels;
using System.Text;
using SimpleSchedulerApiModels.Request.Workers;
using SimpleSchedulerApiModels.Reply.Workers;

namespace SimpleSchedulerServiceChecker;

public class Worker
    : BackgroundService
{
    private static readonly Timer _timer = new(TimeSpan.FromMinutes(20).TotalMilliseconds);
    private readonly string _apiUrl;
    private readonly string[] _serviceNames;
    private readonly ILogger<Worker> _logger;

    public Worker(
        IConfiguration config, 
        ILogger<Worker> logger)
    {
        _apiUrl = config["ApiUrl"];
        _serviceNames = config.GetSection("ServiceNames")
            .GetChildren()
            .Select(x => x.Value)
            .ToArray();
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
                _logger.LogInformation("Checking services: {serviceNames}", string.Join(",", _serviceNames));

                foreach (var serviceName in _serviceNames)
                {
                    if (!IsRunning(serviceName))
                    {
                        _logger.LogCritical("Service {serviceName} is not running", serviceName);
                    }
                }

                using HttpClient client = new();

                HttpResponseMessage response = await client.PostAsJsonAsync(
                    $"{_apiUrl}/Jobs/GetOverdueJobs", 
                    new GetOverdueJobsRequest(),
                    cancellationToken: cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    GetOverdueJobsReply? reply = await response.Content.ReadFromJsonAsync<GetOverdueJobsReply>(cancellationToken: cancellationToken);
                    if (reply is null || reply.Jobs is null)
                    {
                        throw new ApplicationException(string.Format(
                            "Error calling GetOverdueJobs - reply is {0} and reply.Jobs is {1}",
                            reply is null ? "null" : "not null",
                            reply?.Jobs is null ? "null" : "not null"));
                    }

                    if (reply.Jobs.Any())
                    {
                        response = await client.PostAsJsonAsync(
                            $"{_apiUrl}/Workers/GetAllWorkers",
                            new GetAllWorkersRequest(),
                            cancellationToken: cancellationToken);

                        GetAllWorkersReply? workersReply = await response.Content.ReadFromJsonAsync<GetAllWorkersReply>(cancellationToken: cancellationToken);
                        List<WorkerWithSchedules> workers = new();
                        if (workersReply is not null && workersReply.Workers is not null)
                        {
                            workers.AddRange(workersReply.Workers.Where(w => w.Schedules is not null));
                        }

                        var message = new StringBuilder("OVERDUE JOBS\n\n");
                        foreach (var job in reply!.Jobs)
                        {
                            WorkerWithSchedules? worker = workers.FirstOrDefault(w => w.Schedules.Any(s => s.ID == job.ScheduleID));
                            string workerName = worker?.Worker.WorkerName ?? "[unknown worker]";
                            message.AppendLine($"{workerName}");
                            message.AppendLine($"Queued on {job.QueueDateUTC:yyyy\\-MM\\-dd HH\\:mm\\:ss} (UTC)");
                            message.AppendLine($"Status: {job.StatusCode}");
                            if (job.StatusCode == "ERR")
                            {
                                Guid acknowledgementCode = job.AcknowledgementCode;
                                string url = $"{_apiUrl}/acknowledge-error/{acknowledgementCode:N}";
                                message.AppendLine($"Acknowledge: {url}");
                            }
                            message.AppendLine("-----------------------------------");
                        }
                        _logger.LogCritical("Error: {message}", message);
                    }
                }
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
            _logger.LogCritical(ex, "{machineName}: {serviceName} is not running or there is an error",
                Environment.MachineName, serviceName);
            return false;
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
