using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using RunProcessAsTask;

namespace SimpleSchedulerService
{
    public sealed class RunnerWorker
    {
        private readonly SimpleSchedulerModels.Worker _worker;
        private readonly string _workingDirectory;
        private readonly string _executable;

        public RunnerWorker(IConfiguration config, SimpleSchedulerModels.Worker worker)
        {
            _worker = worker;
            _workingDirectory = new DirectoryInfo(Path.Combine(config["WorkerPath"], worker.DirectoryName)).FullName;
            _executable = Path.Combine(_workingDirectory, worker.Executable);
        }

        public async Task<WorkerResult> RunAsync(CancellationToken cancellationToken)
        {
            Trace.WriteLine($"Executing {_executable}");
            var processStartInfo = new ProcessStartInfo
            {
                FileName = _executable,
                Arguments = _worker.ArgumentValues,
                WorkingDirectory = _workingDirectory
            };

            int exitCode;
            string? output;
            string? error;
            try
            {
                using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(_worker.TimeoutMinutes));
                var result = await ProcessEx.RunAsync(processStartInfo, cancellationTokenSource.Token);
                exitCode = result.ExitCode;
                output = string.Join("\n", result.StandardOutput ?? Array.Empty<string>());
                error = string.Join("\n", result.StandardError ?? Array.Empty<string>());
            }
            catch (OperationCanceledException)
            {
                exitCode = -1;
                output = "";
                error = $"Timeout: {_worker.TimeoutMinutes} minutes";
            }
            catch (Exception ex)
            {
                exitCode = -1;
                output = "";
                error = ex.ToString();
            }

            bool success;
            string? detailedMessage;
            if (exitCode == 0)
            {
                success = true;
                detailedMessage = output;
            }
            else
            {
                success = false;
                detailedMessage = $"{output}\n\n{error}";
            }

            return new WorkerResult(success, detailedMessage);
        }
    }
}
