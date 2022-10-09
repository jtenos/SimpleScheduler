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
using System.Reflection;

namespace SimpleSchedulerServiceChecker;

public class Worker
    : BackgroundService
{
    private readonly Timer _timer;
    private readonly ServiceClient _serviceClient;
    private readonly IEmailer _emailer;
    private readonly string[] _serviceNames;
    private readonly ILogger<Worker> _logger;
    private readonly string _appUrl;

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
        _emailer = emailer;
        _appUrl = config["AppUrl"];

        int timerMinutes = config.GetValue<int>("TimerMinutes");
        if (timerMinutes <= 0) { timerMinutes = 20; }
        _logger.LogInformation("Timer set for {timerMinutes} minutes", timerMinutes);
        _timer = new(TimeSpan.FromMinutes(timerMinutes).TotalMilliseconds);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.CompletedTask;

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await _emailer.SendEmailToAdminAsync(
            subject: $"SimpleSchedulerServiceChecker started on {Environment.MachineName}",
            bodyHTML: Assembly.GetExecutingAssembly().Location
        );

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
                        _logger.LogWarning("Service {serviceName} is not running on {machineName}",
                            serviceName, Environment.MachineName);
                        await _emailer.SendEmailToAdminAsync(
                            subject: $"Service {serviceName} is not running on {Environment.MachineName}",
                            bodyHTML: Assembly.GetExecutingAssembly().Location
                        );
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
                    new(IncludeInactiveSchedules: true)
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

                _logger.LogDebug("Workers: {0}", System.Text.Json.JsonSerializer.Serialize(workers));

                StringBuilder message = new("OVERDUE JOBS:<br><br>");
                foreach (var job in jobsReply.Jobs)
                {
                    WorkerWithSchedules? worker = workers.FirstOrDefault(w => w.Schedules.Any(s => s.ID == job.ScheduleID));
                    string workerName = worker?.Worker.WorkerName ?? "[unknown worker]";
                    message.Append($"{workerName}<br>");
                    message.Append($"Queued on {job.QueueDateUTC:yyyy\\-MM\\-dd HH\\:mm\\:ss} (UTC)<br>");
                    message.Append($"Status: {job.StatusCode}<br>");
                    if (job.StatusCode == "ERR")
                    {
                        Guid acknowledgementCode = job.AcknowledgementCode;
                        string url = $"{_appUrl}acknowledge-error/{acknowledgementCode:N}";
                        message.Append($"Acknowledge: <a href='{url}'>{url}</a><br>");
                    }
                    message.Append("-----------------------------------<br>");
                }
                _logger.LogWarning("Error: {message}", message);
                await _emailer.SendEmailToAdminAsync(
                    subject: $"Overdue scheduled jobs on {Environment.MachineName}",
                    bodyHTML: message.ToString()
                );
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
