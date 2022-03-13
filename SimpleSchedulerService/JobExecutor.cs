using SimpleSchedulerApiModels;
using SimpleSchedulerApiModels.Reply.Jobs;
using SimpleSchedulerApiModels.Request.Jobs;
using SimpleSchedulerServiceClient;
using System.Collections.Immutable;
using System.Reflection;

namespace SimpleSchedulerService;

public sealed class JobExecutor
{
    private readonly ILogger<JobExecutor> _logger;
    private readonly IConfiguration _config;
    private readonly ServiceClient _serviceClient;

    public JobExecutor(ILogger<JobExecutor> logger, IConfiguration config, ServiceClient serviceClient)
    {
        _logger = logger;
        _config = config;
        _serviceClient = serviceClient;
    }

    public async Task RestartStuckAppsAsync(CancellationToken cancellationToken)
    {
        (Error? error, RestartStuckJobsReply? reply) = await _serviceClient.PostAsync<RestartStuckJobsRequest, RestartStuckJobsReply>(
            "Jobs/RestartStuckJobs",
            new()
        );

        if (error is not null)
        {
            await SendEmailToAdminAsync("ERROR: Error calling RestartStuckJobs",
                error.Message, cancellationToken);
            return;
        }
    }

    public async Task GoAsync(CancellationToken cancellationToken)
    {
        try
        {
            (Error? error, StartDueJobsReply? startReply) = await _serviceClient.PostAsync<StartDueJobsRequest, StartDueJobsReply>(
                "Jobs/StartDueJobs",
                new()
            );

            if (error is not null)
            {
                throw new ApplicationException(error.Message);
            }

            if (startReply!.NumRunningJobs >= 10)
            {
                return;
            }

            (error, DequeueScheduledJobsReply? dequeueReply) = await _serviceClient.PostAsync<DequeueScheduledJobsRequest, DequeueScheduledJobsReply>(
                "Jobs/DequeueScheduledJobs",
                new()
            );

            if (error is not null)
            {
                throw new ApplicationException(error.Message);
            }

            foreach (var job in dequeueReply!.Jobs)
            {
                try
                {
                    _logger.LogInformation("Processing job {jobID}", job.ID);

                    var worker = new RunnerWorker(_config, job.Worker);

                    _logger.LogInformation("Executing job {JobID}", job.ID);
                    _logger.LogInformation("Worker={worker}", job.Worker.WorkerName);

                    worker.RunAsync(cancellationToken).ContinueWith(async t =>
                    {
                        if (t.IsFaulted)
                        {
                            _logger.LogInformation("IsFaulted");
                            await CompleteWorker(job, new(Success: false,
                                t.Exception!.ToString()), cancellationToken).ConfigureAwait(false);
                        }
                        else if (t.IsCanceled)
                        {
                            _logger.LogInformation("IsCanceled");
                            await CompleteWorker(job, new(Success: false,
                                "Cancelled for an unknown reason"), cancellationToken).ConfigureAwait(false);
                        }
                        else if (t.IsCompleted)
                        {
                            _logger.LogInformation("IsCompleted");
                            await CompleteWorker(job, t.Result, cancellationToken).ConfigureAwait(false);
                        }
                        else
                        {
                            _logger.LogInformation("ELSE");
                        }
                    }, TaskScheduler.Default).DoNotAwait();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error executing job {jobID}", job.ID);
                    await CompleteWorker(
                        job,
                        new(Success: false, ex.ToString()),
                        cancellationToken
                    ).ConfigureAwait(false);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Error in GoAsync");
        }
    }

    private async Task CompleteWorker(JobWithWorker jobDetail, WorkerResult workerResult, CancellationToken cancellationToken)
    {
        // https://github.com/dotnet/runtime/issues/43970
        IServiceScope scope = default!;
        try
        {
            scope = _scopeFactory.CreateScope();
            var jobManager = scope.ServiceProvider.GetRequiredService<IJobManager>();

            Debug.WriteLine($"Completing JobID={jobDetail.Job.JobID} with success flag={workerResult.Success}");
            try
            {
                await jobManager.CompleteJobAsync(jobDetail.Job.JobID,
                    workerResult.Success,
                    workerResult.DetailedMessage, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                throw;
            }
            try
            {
                await SendEmailAsync(jobDetail, workerResult, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
            }
            Interlocked.Decrement(ref _runningTasks);
        }
        catch
        {
            scope.ServiceProvider.GetRequiredService<DatabaseFactory>().MarkForRollback();
        }
        finally
        {
            await ((IAsyncDisposable)scope).DisposeAsync().ConfigureAwait(false);
        }
    }

    private async Task SendEmailAsync(JobDetail jobDetail, WorkerResult workerResult, CancellationToken cancellationToken)
    {
        try
        {
            HashSet<string> toAddresses = new();
            if (!string.IsNullOrWhiteSpace(jobDetail.Worker.EmailOnSuccess))
            {
                // Always send to the EmailOnSuccess group, for successes and failures
                foreach (string addr in (jobDetail.Worker.EmailOnSuccess ?? "").Split(';').Where(x => !string.IsNullOrWhiteSpace(x)))
                {
                    toAddresses.Add(addr);
                }
            }

            if (!workerResult.Success)
            {
                // For failures, send to the admin
                foreach (string addr in _config["MailSettings:AdminEmail"].Split(';').Where(x => !string.IsNullOrWhiteSpace(x)))
                {
                    toAddresses.Add(addr);
                }
            }

            if (!toAddresses.Any())
            {
                return;
            }

            string subject = $"[{_config["EnvironmentName"]}] {(workerResult.Success ? "SUCCESS" : "ERROR")} - Worker: [{jobDetail.Worker.WorkerName}]";
            string detailedMessage = (workerResult.DetailedMessage ?? "").Replace("\r\n", "<br>").Replace("\r", "<br>").Replace("\n", "<br>");
            string body = $"Job ID: {jobDetail.Job.JobID}<br><br>{detailedMessage}";

            string url = _config["ApplicationURL"];
            while (!url.EndsWith("/"))
            {
                url = $"{url}/";
            }
            url += $"ExecuteAction?ActionID={jobDetail.Job.AcknowledgementID:N}";
            if (!workerResult.Success)
            {
                body = $"<a href='{url}' target=_blank>Acknowledge error</a><br><br>{body}";
            }

            await SendEmailAsync(subject, body, toAddresses, cancellationToken);
        }
        catch (Exception ex)
        {
            Trace.TraceError($"Error sending email: {ex}");
        }
    }

    private async Task SendEmailToAdminAsync(string subject, string htmlBody, CancellationToken cancellationToken)
    {
        await SendEmailAsync(subject, htmlBody, _config["AdminEmail"].Split(';'), cancellationToken);
    }

    private async Task SendEmailAsync(string subject, string htmlBody, IEnumerable<string> toAddresses, CancellationToken cancellationToken)
    {
        if (!toAddresses.Any()) { return; }
        Trace.TraceInformation($"Sending email to {string.Join(";", toAddresses)}");

        await _emailer.SendEmailAsync(toAddresses, subject, htmlBody, cancellationToken).ConfigureAwait(false);

        Trace.TraceInformation("Email sent");
    }
}
