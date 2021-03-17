using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace SimpleSchedulerService
{
    public sealed class JobScheduler
        : IDisposable
    {
        private readonly IConfiguration _config;
        private readonly JobExecutor _jobExecutor;

        private readonly System.Timers.Timer _timer = new(5000);

        public JobScheduler(IConfiguration config, JobExecutor jobExecutor) => (_config, _jobExecutor) = (config, jobExecutor);

        public async Task StartSchedulerAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (!Directory.Exists(_config["WorkerPath"]))
                {
                    Directory.CreateDirectory(_config["WorkerPath"]);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
            }
            Trace.WriteLine($"WorkerPath={_config["WorkerPath"]}");
            Trace.WriteLine($"connectionString={_config.GetConnectionString("scheduler")}");

            await _jobExecutor.RestartStuckAppsAsync(cancellationToken).ConfigureAwait(false);

            _timer.Elapsed += async (sender, e) =>
            {
                _timer.Stop();
                try
                {
                    await _jobExecutor.GoAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Trace.TraceError(ex.ToString());
                }
                _timer.Start();
            };
            _timer.Start();
        }

        public void Dispose() => _timer.Dispose();
    }
}
