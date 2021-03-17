using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleSchedulerBusiness;
using SimpleSchedulerData;
using SimpleSchedulerEmail;
using SimpleSchedulerModels;

namespace SimpleSchedulerService
{
    public sealed class JobExecutor
    {
        private readonly IConfiguration _config;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IEmailer _emailer;

        private static int _runningTasks;

        public JobExecutor(IConfiguration config, IServiceScopeFactory scopeFactory, IEmailer emailer)
            => (_config, _scopeFactory, _emailer) = (config, scopeFactory, emailer);

        public async Task RestartStuckAppsAsync(CancellationToken cancellationToken)
        {
            // https://github.com/dotnet/runtime/issues/43970
            IServiceScope scope = default!;
            try
            {
                scope = _scopeFactory.CreateScope();
                var jobManager = scope.ServiceProvider.GetRequiredService<IJobManager>();
                Trace.TraceInformation("Restarting stuck apps");
                await jobManager.RestartStuckJobsAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                scope.ServiceProvider.GetRequiredService<IDatabaseFactory>().MarkForRollback();
                if (!string.IsNullOrWhiteSpace(_config["AdminEmail"]))
                {
                    await SendEmailAsync("ERROR: Error calling applicationStarted",
                        ex.ToString(), _config["AdminEmail"].Split(';'), cancellationToken);
                }
                throw;
            }
            finally
            {
                await ((IAsyncDisposable)scope).DisposeAsync().ConfigureAwait(false);
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
                        schedule.Schedule.TimeOfDayUTC, schedule.Schedule.RecurTime,
                        schedule.Schedule.RecurBetweenStartUTC, schedule.Schedule.RecurBetweenEndUTC);
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
                        workerResult.Success ? "SUC" : "ERR",
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
                    foreach (string addr in _config["AdminEmail"].Split(';').Where(x => !string.IsNullOrWhiteSpace(x)))
                    {
                        toAddresses.Add(addr);
                    }
                }

                if (!toAddresses.Any())
                {
                    return;
                }

                string subject = $"[{_config["EnvironmentName"]}] {(workerResult.Success ? "SUCCESS" : "ERROR")}";
                string detailedMessage = (workerResult.DetailedMessage ?? "").Replace("\r\n", "<br>").Replace("\r", "<br>").Replace("\n", "<br>");
                string body = $"Job ID: {jobDetail.Job.JobID}<br><br>{detailedMessage}";

                string url = _config["ApplicationURL"];
                while (!url.EndsWith("/"))
                {
                    url = $"{url}/";
                }
                url += $"ExecuteAction?ActionID={jobDetail.Job.AcknowledgementID:N}";
                body = $"<a href='{url}' target=_blank>Acknowledge error</a><br><br>{body}";

                await SendEmailAsync(subject, body, toAddresses, cancellationToken);
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Error sending email: {ex}");
            }
        }

        private async Task SendEmailAsync(string subject, string htmlBody, IEnumerable<string> toAddresses, CancellationToken cancellationToken)
        {
            Trace.TraceInformation($"Sending email to {string.Join(";", toAddresses)}");

            await _emailer.SendEmailAsync(toAddresses, subject, htmlBody, cancellationToken).ConfigureAwait(false);

            Trace.TraceInformation("Email sent");
        }
    }
}
