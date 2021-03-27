using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Timer = System.Timers.Timer;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.ServiceProcess;
using Microsoft.Extensions.DependencyInjection;
using SimpleSchedulerEmail;
using SimpleSchedulerBusiness;
using SimpleSchedulerData;

namespace SimpleSchedulerServiceChecker
{
    public class Worker
        : BackgroundService
    {
        private static readonly Timer _timer = new(TimeSpan.FromMinutes(20).TotalMilliseconds);
        private readonly IConfiguration _config;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IEmailer _emailer;
        private static string[]? _serviceNames;

        public Worker(IConfiguration config, IServiceScopeFactory scopeFactory, IEmailer emailer)
            => (_config, _scopeFactory, _emailer) = (config, scopeFactory, emailer);

        protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.CompletedTask;

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            async Task GoAsync()
            {
                try
                {
                    _serviceNames = _config.GetSection("ServiceNames").GetChildren().Select(x => x.Value).ToArray();

                    foreach (var serviceName in _serviceNames!)
                    {
                        if (!IsRunning(serviceName))
                        {
                            await SendEmailAsync($"Service {serviceName} is not running", cancellationToken);
                        }
                    }

                    // https://github.com/dotnet/runtime/issues/43970
                    IServiceScope scope = default!;
                    try
                    {
                        scope = _scopeFactory.CreateScope();
                        var jobManager = scope.ServiceProvider.GetRequiredService<IJobManager>();
                        var overdueJobs = await jobManager.GetOverdueJobsAsync(cancellationToken).ConfigureAwait(false);
                        if (overdueJobs.Any())
                        {
                            var message = new StringBuilder("OVERDUE JOBS<br><br>");
                            foreach (var job in overdueJobs)
                            {
                                message.Append($"{job.Worker.WorkerName}: ");
                                message.Append($"Queued on {job.Job.QueueDateUTC:yyyy\\-MM\\-dd HH\\:mm\\:ss} (UTC), ");
                                message.Append($"Status: {job.Job.StatusCode}");
                                if (job.Job.StatusCode == "ERR")
                                {
                                    Guid acknowledgeActionID = job.Job.AcknowledgementID;
                                    string url = _config["ApplicationURL"];
                                    if (!url.EndsWith("/"))
                                    {
                                        url = $"{url}/";
                                    }
                                    url += $"ExecuteAction?ActionID={acknowledgeActionID:N}";
                                    message.Append($" <a href='{url}' target=_blank>Acknowledge</a>");
                                }
                                message.Append("<br>");
                            }
                            await SendEmailAsync(message.ToString(), cancellationToken);
                        }
                    }
                    catch
                    {
                        scope.ServiceProvider.GetRequiredService<IDatabaseFactory>().MarkForRollback();
                    }
                    finally
                    {
                        await ((IAsyncDisposable)scope).DisposeAsync().ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    await SendEmailAsync($"EXCEPTION\n{ex}", cancellationToken);
                }
            }
            _timer.Elapsed += async (sender, e) => await GoAsync().ConfigureAwait(false);
            _timer.Enabled = true;
            await GoAsync().ConfigureAwait(false);
        }

        private static bool IsRunning(string serviceName)
        {
            try
            {
                // This only runs on Windows, so ignore the platform rule
#pragma warning disable CA1416 // Validate platform compatibility
                return new ServiceController(serviceName).Status == ServiceControllerStatus.Running;
#pragma warning restore CA1416 // Validate platform compatibility
            }
            catch
            {
                return false;
            }
        }

        private async Task SendEmailAsync(string bodyHTML, CancellationToken cancellationToken)
        {
            try
            {
                await _emailer.SendEmailToAdminAsync($"{Environment.MachineName}: Service Checker Alert", bodyHTML, cancellationToken);
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
