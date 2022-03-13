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

    private static int _runningTasks;

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
        // https://github.com/dotnet/runtime/issues/43970
        IServiceScope scope = default!;
        try
        {
            scope = _scopeFactory.CreateScope();
            var scheduleManager = scope.ServiceProvider.GetRequiredService<IScheduleManager>();
            var jobManager = scope.ServiceProvider.GetRequiredService<IJobManager>();
            var schedulesToInsert = await scheduleManager.GetSchedulesToInsertAsync(cancellationToken).ConfigureAwait(false);

            foreach (var schedule in schedulesToInsert)
            {
                Trace.TraceInformation($"Inserting job for schedule {schedule.Schedule.ScheduleID} (Worker {schedule.Schedule.WorkerID})");
                var lastQueuedJob = await jobManager.GetLastQueuedJobAsync(schedule.Schedule.ScheduleID,
                    cancellationToken).ConfigureAwait(false);
                DateTime? lastQueueDate = lastQueuedJob?.QueueDateUTC;

                var daysOfTheWeek = new List<DayOfWeek>();
                if (schedule.Schedule.Sunday) daysOfTheWeek.Add(DayOfWeek.Sunday);
                if (schedule.Schedule.Monday) daysOfTheWeek.Add(DayOfWeek.Monday);
                if (schedule.Schedule.Tuesday) daysOfTheWeek.Add(DayOfWeek.Tuesday);
                if (schedule.Schedule.Wednesday) daysOfTheWeek.Add(DayOfWeek.Wednesday);
                if (schedule.Schedule.Thursday) daysOfTheWeek.Add(DayOfWeek.Thursday);
                if (schedule.Schedule.Friday) daysOfTheWeek.Add(DayOfWeek.Friday);
                if (schedule.Schedule.Saturday) daysOfTheWeek.Add(DayOfWeek.Saturday);

                var newQueueDate = ScheduleFinder.GetNextDate(lastQueueDate, daysOfTheWeek.ToImmutableArray(),
                    schedule.Schedule.TimeOfDayUTC?.AsTimeSpan(),
                    schedule.Schedule.RecurTime?.AsTimeSpan(),
                    schedule.Schedule.RecurBetweenStartUTC?.AsTimeSpan(),
                    schedule.Schedule.RecurBetweenEndUTC?.AsTimeSpan());
                await jobManager.AddJobAsync(schedule.Schedule.ScheduleID, newQueueDate, cancellationToken).ConfigureAwait(false);
            }

            if (_runningTasks >= 10)
            {
                return;
            }

            var items = await jobManager.DequeueScheduledJobsAsync(cancellationToken).ConfigureAwait(false);

            foreach (var jobDetail in items)
            {
                try
                {
                    Trace.TraceInformation($"Processing job {jobDetail.Job.JobID}");
                    Interlocked.Increment(ref _runningTasks);

                    var worker = new RunnerWorker(_config, jobDetail.Worker);

                    // TODO: Auto-map this
                    foreach (var prop in typeof(RunnerWorker).GetProperties(BindingFlags.Public | BindingFlags.Instance))
                    {
                        var jobDetailProp = jobDetail.GetType().GetProperty(prop.Name)!;
                        try
                        {
                            prop.SetValue(worker, jobDetailProp.GetValue(jobDetail, null), null);
                        }
                        catch
                        {
                            Debug.WriteLine($"Exception setting {prop.Name}");
                            throw;
                        }
                    }

                    Trace.TraceInformation($"Executing job {jobDetail.Job.JobID}");
                    Trace.TraceInformation($"Worker={worker}");
                    worker.RunAsync(cancellationToken).ContinueWith(async t =>
                    {
                        if (t.IsFaulted)
                        {
                            Trace.TraceInformation("IsFaulted");
                            await CompleteWorker(jobDetail, new WorkerResult(Success: false,
                                t.Exception!.ToString()), cancellationToken).ConfigureAwait(false);
                        }
                        else if (t.IsCanceled)
                        {
                            Trace.TraceInformation("IsCanceled");
                            await CompleteWorker(jobDetail, new WorkerResult(Success: false,
                                "Cancelled for an unknown reason"), cancellationToken).ConfigureAwait(false);
                        }
                        else if (t.IsCompleted)
                        {
                            Trace.TraceInformation("IsCompleted");
                            await CompleteWorker(jobDetail, t.Result, cancellationToken).ConfigureAwait(false);
                        }
                        else
                        {
                            Trace.TraceInformation("ELSE");
                        }
                    }, TaskScheduler.Default).DoNotAwait();
                }
                catch (Exception ex)
                {
                    Trace.TraceError(ex.ToString());
                    await CompleteWorker(jobDetail, new WorkerResult(Success: false, ex.ToString()), cancellationToken).ConfigureAwait(false);
                }
            }
        }
        catch (Exception ex)
        {
            Trace.TraceError(ex.ToString());
            scope.ServiceProvider.GetRequiredService<DatabaseFactory>().MarkForRollback();
        }
        finally
        {
            await ((IAsyncDisposable)scope).DisposeAsync().ConfigureAwait(false);
        }
    }

    private async Task CompleteWorker(JobDetail jobDetail, WorkerResult workerResult, CancellationToken cancellationToken)
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
