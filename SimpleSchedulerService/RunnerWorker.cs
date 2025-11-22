using RunProcessAsTask;
using System.Diagnostics;

namespace SimpleSchedulerService;

public sealed class RunnerWorker
{
    private readonly ILogger<RunnerWorker> _logger;
    private readonly SimpleSchedulerApiModels.Worker _worker;
    private readonly string _workingDirectory;
    private readonly string _executable;
    private readonly long _jobId;
    private readonly string _workerPath;
    private readonly object _writerLock = new();

    public RunnerWorker(
        IServiceProvider serviceProvider,
        IConfiguration config,
        SimpleSchedulerApiModels.Worker worker,
        long jobId
    )
    {
        _logger = serviceProvider.GetRequiredService<ILogger<RunnerWorker>>();
        _worker = worker;
        _workerPath = config["WorkerPath"]!;
        _workingDirectory = new DirectoryInfo(Path.Combine(_workerPath, worker.DirectoryName)).FullName;
        _executable = Path.Combine(_workingDirectory, worker.Executable);
        _jobId = jobId;
    }

    public async Task<WorkerResult> RunAsync()
    {
        _logger.LogInformation("Executing {executable}", _executable);
        
        // Set up live output file
        DirectoryInfo liveOutputDir = new(Path.Combine(_workerPath, "__live_output__"));
        liveOutputDir.Create();
        FileInfo liveOutputFile = new(Path.Combine(liveOutputDir.FullName, $"{_jobId}.txt"));
        
        ProcessStartInfo processStartInfo = new()
        {
            FileName = _executable,
            Arguments = _worker.ArgumentValues,
            WorkingDirectory = _workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        int exitCode;
		List<string> standardOutput = new();
		List<string> standardError = new();
		
		StreamWriter? liveWriter = null;
		try
		{
            // Open file for writing live output
            liveWriter = new StreamWriter(liveOutputFile.FullName, false, System.Text.Encoding.UTF8)
            {
                AutoFlush = true
            };

            using CancellationTokenSource cancellationTokenSource = new(TimeSpan.FromMinutes(_worker.TimeoutMinutes));
            using var process = new Process { StartInfo = processStartInfo };
            
            // Capture stdout
            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    standardOutput.Add(e.Data);
                    lock (_writerLock)
                    {
                        try
                        {
                            liveWriter.WriteLine(e.Data);
                        }
                        catch
                        {
                            // Ignore write errors to live file
                        }
                    }
                }
            };
            
            // Capture stderr
            process.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    standardError.Add(e.Data);
                    lock (_writerLock)
                    {
                        try
                        {
                            liveWriter.WriteLine($"ERROR: {e.Data}");
                        }
                        catch
                        {
                            // Ignore write errors to live file
                        }
                    }
                }
            };
            
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            
            await process.WaitForExitAsync(cancellationTokenSource.Token);
            exitCode = process.ExitCode;
        }
        catch (OperationCanceledException)
        {
            exitCode = -1;
            string timeoutMsg = $"Timeout: {_worker.TimeoutMinutes} minutes";
            standardError.Add(timeoutMsg);
            lock (_writerLock)
            {
                try
                {
                    liveWriter?.WriteLine($"ERROR: {timeoutMsg}");
                }
                catch
                {
                    // Ignore write errors to live file
                }
            }
        }
        catch (Exception ex)
        {
            exitCode = -1;
            standardError.Add(ex.ToString());
            lock (_writerLock)
            {
                try
                {
                    liveWriter?.WriteLine($"ERROR: {ex}");
                }
                catch
                {
                    // Ignore write errors to live file
                }
            }
        }
        finally
        {
            liveWriter?.Dispose();
        }
        
		string? output = string.Join("\n", standardOutput);
		string? error = string.Join("\n", standardError);

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
