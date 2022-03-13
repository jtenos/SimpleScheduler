using RunProcessAsTask;
using System.Diagnostics;

namespace SimpleSchedulerService;

public sealed class RunnerWorker
{
    private readonly ILogger<RunnerWorker> _logger;
    private readonly SimpleSchedulerApiModels.Worker _worker;
    private readonly string _workingDirectory;
    private readonly string _executable;

    public RunnerWorker(
        IServiceProvider serviceProvider,
        IConfiguration config,
        SimpleSchedulerApiModels.Worker worker
    )
    {
        _logger = serviceProvider.GetRequiredService<ILogger<RunnerWorker>>();
        _worker = worker;
        _workingDirectory = new DirectoryInfo(Path.Combine(config["WorkerPath"], worker.DirectoryName)).FullName;
        _executable = Path.Combine(_workingDirectory, worker.Executable);
    }

    public async Task<WorkerResult> RunAsync()
    {
        _logger.LogInformation("Executing {executable}", _executable);
        ProcessStartInfo processStartInfo = new()
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
            using CancellationTokenSource cancellationTokenSource = new(TimeSpan.FromMinutes(_worker.TimeoutMinutes));
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

        return new(success, detailedMessage);
    }
}
