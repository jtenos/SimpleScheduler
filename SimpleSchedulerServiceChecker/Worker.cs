using Timer = System.Timers.Timer;
using System.ServiceProcess;
using SimpleSchedulerApiModels.Reply.Jobs;
using SimpleSchedulerApiModels.Request.Jobs;
using SimpleSchedulerApiModels;
using System.Text;
using SimpleSchedulerApiModels.Request.Workers;
using SimpleSchedulerApiModels.Reply.Workers;
using SimpleSchedulerServiceClient;
using SimpleSchedulerEmail;

namespace SimpleSchedulerServiceChecker;

public class Worker
    : BackgroundService
{
    private static readonly Timer _timer = new(TimeSpan.FromMinutes(20).TotalMilliseconds);
    private readonly ServiceClient _serviceClient;
    private readonly string[] _serviceNames;
    private readonly ILogger<Worker> _logger;
    private readonly string _apiUrl;

    public Worker(
        IEmailer emailer, // forces DI loading
        IConfiguration config,
        ILogger<Worker> logger,
        ServiceClient serviceClient)
    {
        System.Diagnostics.Debug.WriteLine(emailer);

        _serviceNames = config.GetSection("ServiceNames")
            .GetChildren()
            .Select(x => x.Value)
            .ToArray();
        _logger = logger;
        _serviceClient = serviceClient;
        _apiUrl = config["ApiUrl"];
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.CompletedTask;

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogCritical("SimpleSchedulerServiceChecker started on {machineName}", Environment.MachineName);
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

                _logger.LogInformation("Looking for overdue jobs", string.Join(",", _serviceNames));
                (Error? error, GetOverdueJobsReply? jobsReply) = await _serviceClient.PostAsync<GetOverdueJobsRequest, GetOverdueJobsReply>(
                    "Jobs/GetOverdueJobs",
                    new()
                );

                if (error is not null)
                {
                    throw new ApplicationException(error.Message);
                }

                if (jobsReply?.Jobs is null)
                {
                    throw new ApplicationException(string.Format(
                        "Error calling GetOverdueJobs - jobsReply is {0} and jobsReply.Jobs is {1}",
                        jobsReply is null ? "null" : "not null",
                        jobsReply?.Jobs is null ? "null" : "not null")
                    );
                }

                if (!jobsReply.Jobs.Any())
                {
                    _logger.LogInformation("No overdue jobs found");
                    return;
                }

                (error, GetAllWorkersReply? workersReply) = await _serviceClient.PostAsync<GetAllWorkersRequest, GetAllWorkersReply>(
                    "Workers/GetAllWorkers",
                    new()
                );

                if (error is not null)
                {
                    throw new ApplicationException(error.Message);
                }

                if (workersReply?.Workers is null)
                {
                    throw new ApplicationException(string.Format(
                        "Error calling GetAllWorkers - workersReply is {0} and workersReply.Workers is {1}",
                        workersReply is null ? "null" : "not null",
                        workersReply?.Workers is null ? "null" : "not null")
                    );
                }

                WorkerWithSchedules[] workers = workersReply.Workers.Where(w => w.Schedules is not null).ToArray();

                var message = new StringBuilder("OVERDUE JOBS\n\n");
                foreach (var job in jobsReply.Jobs)
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
                _logger.LogCritical("Error: {message}", message.Replace("\r\n", "<br>").Replace("\n", "<br>"));
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Error checking scheduler services");
            }
        }
        _timer.Elapsed += async (sender, e) => await GoAsync();
        _timer.Enabled = true;
        await GoAsync();
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
