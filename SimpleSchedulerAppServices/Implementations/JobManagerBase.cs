using System.Text;
using Microsoft.Extensions.Logging;
using SimpleSchedulerAppServices.Interfaces;
using SimpleSchedulerAppServices.Utilities;
using SimpleSchedulerData;
using SimpleSchedulerDataEntities;
using SimpleSchedulerDomainModels;
using SimpleSchedulerEmail;

namespace SimpleSchedulerAppServices.Implementations;

/// <summary>
/// Database-agnostic logic for the job manager (email, message file compression, overdue filtering,
/// schedule-driven job insertion). The provider-specific data access lives in the abstract Core
/// methods, implemented by the SqlServer and Sqlite subclasses.
/// </summary>
public abstract class JobManagerBase : IJobManager
{
    protected IDatabase Db { get; }
    private readonly ILogger _logger;
    private readonly IEmailer _emailer;

    protected JobManagerBase(ILogger logger, IDatabase db, IEmailer emailer)
    {
        _logger = logger;
        Db = db;
        _emailer = emailer;
    }

    // A class with settable properties (not a positional record) so Dapper can map SQLite's
    // INTEGER 0/1 result columns to bool (constructor injection doesn't allow that conversion).
    protected sealed class CancelJobResult
    {
        public bool Success { get; set; }
        public bool AlreadyCompleted { get; set; }
        public bool AlreadyStarted { get; set; }
    }

    // ---- provider-specific data access ----
    protected abstract Task RestartStuckJobsCoreAsync();
    protected abstract Task AcknowledgeErrorCoreAsync(Guid acknowledgementCode);
    protected abstract Task<JobEntity> GetJobEntityAsync(long id);
    protected abstract Task<CancelJobResult> CancelJobCoreAsync(long jobID);
    protected abstract Task CompleteJobCoreAsync(long id, bool success, bool hasDetailedMessage);
    protected abstract Task<(JobEntity[] Jobs, WorkerEntity[] Workers)> GetJobWithWorkerCoreAsync(long id);
    protected abstract Task<JobWithWorkerIDEntity[]> SearchJobsCoreAsync(
        string? statusCode, long? workerID, string? workerName, bool overdueOnly, int offset, int numRows);
    protected abstract Task<ScheduleEntity[]> GetSchedulesByIdsAsync(long[] ids);
    protected abstract Task<WorkerEntity[]> GetWorkersByIdsAsync(long[] ids);
    protected abstract Task<(JobWithWorkerIDEntity[] Jobs, WorkerEntity[] Workers)> DequeueCoreAsync();
    protected abstract Task<ScheduleEntity[]> GetSchedulesForJobInsertionAsync();
    protected abstract Task<JobEntity?> GetMostRecentJobByScheduleAsync(long scheduleID);
    protected abstract Task InsertJobCoreAsync(long scheduleID, DateTime queueDateUTC);

    // ---- agnostic orchestration ----
    async Task IJobManager.AcknowledgeErrorAsync(Guid acknowledgementCode)
        => await AcknowledgeErrorCoreAsync(acknowledgementCode).ConfigureAwait(false);

    async Task IJobManager.CancelJobAsync(long jobID)
    {
        CancelJobResult cancelResult = await CancelJobCoreAsync(jobID).ConfigureAwait(false);

        if (cancelResult.Success) { return; }
        if (cancelResult.AlreadyCompleted) { throw new ApplicationException("Already completed"); }
        if (cancelResult.AlreadyStarted) { throw new ApplicationException("Already started"); }

        throw new ApplicationException("Invalid cancel result");
    }

    async Task IJobManager.CompleteJobAsync(long id, bool success, string? detailedMessage,
        string adminEmail, string appUrl, string environmentName, string workerPath)
    {
        if (!string.IsNullOrWhiteSpace(detailedMessage))
        {
            DirectoryInfo messageDir = new(Path.Combine(workerPath, "__messages__"));
            messageDir.Create();
            messageDir.Refresh();
            FileInfo messageGZipFile = new(Path.Combine(messageDir.FullName, $"{id}.txt.gz"));

            GZipTextFile(messageGZipFile, detailedMessage.Trim());
        }

        await CompleteJobCoreAsync(id, success, !string.IsNullOrWhiteSpace(detailedMessage)).ConfigureAwait(false);

        JobWithWorker jobWithWorker = await GetJobWithWorkerAsync(id).ConfigureAwait(false);

        await SendEmailAsync(
            jobWithWorker: jobWithWorker,
            adminEmail: adminEmail,
            appUrl: appUrl,
            environmentName: environmentName,
            success: success,
            detailedMessage: detailedMessage
        ).ConfigureAwait(false);
    }

    async Task<JobWithWorker[]> IJobManager.DequeueScheduledJobsAsync()
    {
        (JobWithWorkerIDEntity[] jobEntities, WorkerEntity[] workerEntities) =
            await DequeueCoreAsync().ConfigureAwait(false);

        List<JobWithWorker> result = new();
        foreach (JobWithWorkerIDEntity jobEntity in jobEntities)
        {
            WorkerEntity workerEntity = workerEntities.Single(w => w.ID == jobEntity.WorkerID);
            result.Add(ModelBuilders.GetJobWithWorker(jobEntity, workerEntity));
        }
        return result.ToArray();
    }

    async Task<Job> IJobManager.GetJobAsync(long id)
        => ModelBuilders.GetJob(await GetJobEntityAsync(id).ConfigureAwait(false));

    Task<string> IJobManager.GetDetailedMessageAsync(long id, string workerPath)
    {
        DirectoryInfo messageDir = new(Path.Combine(workerPath, "__messages__"));
        messageDir.Create();
        messageDir.Refresh();
        FileInfo messageGZipFile = new(Path.Combine(messageDir.FullName, $"{id}.txt.gz"));
        if (!messageGZipFile.Exists)
        {
            return Task.FromResult("** NO MESSAGE FILE FOUND **");
        }

        string fileContents = UnGZipTextFile(messageGZipFile);
        if (string.IsNullOrWhiteSpace(fileContents))
        {
            return Task.FromResult("** MESSAGE EMPTY **");
        }
        return Task.FromResult(fileContents);
    }

    private static string UnGZipTextFile(FileInfo messageGZipFile)
    {
        using MemoryStream outputStream = new();
        using FileStream fileStream = messageGZipFile.OpenRead();
        GZip.Decompress(fileStream, outputStream);
        return Encoding.UTF8.GetString(outputStream.ToArray());
    }

    private static void GZipTextFile(FileInfo messageGZipFile, string contents)
    {
        using MemoryStream inputStream = new(Encoding.UTF8.GetBytes(contents));
        using FileStream fileStream = messageGZipFile.OpenWrite();
        GZip.Compress(inputStream, fileStream);
    }

    async Task<JobWithWorkerID[]> IJobManager.GetLatestJobsAsync(int pageNumber,
        int rowsPerPage, string? statusCode, long? workerID, string? workerName, bool overdueOnly)
    {
        JobWithWorkerIDEntity[] jobs = await SearchJobsCoreAsync(
            statusCode: statusCode,
            workerID: workerID,
            workerName: workerName,
            overdueOnly: overdueOnly,
            offset: (pageNumber - 1) * rowsPerPage,
            numRows: rowsPerPage
        ).ConfigureAwait(false);

        if (!overdueOnly)
        {
            return jobs.Select(j => ModelBuilders.GetJobWithWorkerID(j)).ToArray();
        }

        // For overdue only, filter RUN/ERR/NEW status, and different levels based on the status
        List<JobWithWorkerIDEntity> filteredJobs = new();

        Dictionary<long, ScheduleEntity> schedules = (await GetSchedulesByIdsAsync(
            jobs.Select(j => j.ScheduleID).Distinct().ToArray()
        ).ConfigureAwait(false))
        .ToDictionary(s => s.ID, s => s);

        Dictionary<long, WorkerEntity> workers = (await GetWorkersByIdsAsync(
            schedules.Select(s => s.Value.WorkerID).Distinct().ToArray()
        ).ConfigureAwait(false))
        .ToDictionary(w => w.ID, w => w);

        foreach (JobWithWorkerIDEntity job in jobs)
        {
            ScheduleEntity schedule = schedules[job.ScheduleID];
            WorkerEntity worker = workers[schedule.WorkerID];

            if (job.StatusCode == Job.STATUS_RUNNING)
            {
                // Greater than the timeout period for the worker
                if (job.QueueDateUTC.AddMinutes(worker.TimeoutMinutes) < DateTime.UtcNow)
                {
                    filteredJobs.Add(job);
                }
            }
            else if (job.StatusCode == Job.STATUS_ERROR)
            {
                // All errors show up here
                filteredJobs.Add(job);
            }
            else if (job.StatusCode == Job.STATUS_NEW)
            {
                // Been stuck in NEW for more than one minute
                if (job.QueueDateUTC.AddMinutes(1) < DateTime.UtcNow)
                {
                    filteredJobs.Add(job);
                }
            }
        }

        return filteredJobs.Select(j => ModelBuilders.GetJobWithWorkerID(j)).ToArray();
    }

    async Task<Job[]> IJobManager.GetOverdueJobsAsync()
    {
        return (await ((IJobManager)this).GetLatestJobsAsync(
            pageNumber: 1,
            rowsPerPage: 999,
            statusCode: null,
            workerID: null,
            workerName: null,
            overdueOnly: true
        ).ConfigureAwait(false))
        .OrderBy(x => x.QueueDateUTC)
        .ToArray();
    }

    async Task IJobManager.RestartStuckJobsAsync()
        => await RestartStuckJobsCoreAsync().ConfigureAwait(false);

    async Task<int> IJobManager.StartDueJobsAsync()
    {
        Schedule[] schedulesToInsert = (await GetSchedulesForJobInsertionAsync().ConfigureAwait(false))
            .Select(s => ModelBuilders.GetSchedule(s))
            .ToArray();

        foreach (Schedule schedule in schedulesToInsert)
        {
            _logger.LogInformation("Inserting job for schedule {scheduleID} (Worker {workerID})",
                schedule.ID, schedule.WorkerID);
            JobEntity? lastQueuedJob = await GetMostRecentJobByScheduleAsync(schedule.ID).ConfigureAwait(false);
            DateTime? lastQueueDate = lastQueuedJob?.QueueDateUTC;

            List<DayOfWeek> daysOfTheWeek = new();
            if (schedule.Sunday) daysOfTheWeek.Add(DayOfWeek.Sunday);
            if (schedule.Monday) daysOfTheWeek.Add(DayOfWeek.Monday);
            if (schedule.Tuesday) daysOfTheWeek.Add(DayOfWeek.Tuesday);
            if (schedule.Wednesday) daysOfTheWeek.Add(DayOfWeek.Wednesday);
            if (schedule.Thursday) daysOfTheWeek.Add(DayOfWeek.Thursday);
            if (schedule.Friday) daysOfTheWeek.Add(DayOfWeek.Friday);
            if (schedule.Saturday) daysOfTheWeek.Add(DayOfWeek.Saturday);

            DateTime newQueueDate = ScheduleFinder.GetNextDate(lastQueueDate, daysOfTheWeek.ToArray(),
                schedule.TimeOfDayUTC,
                schedule.RecurTime,
                schedule.RecurBetweenStartUTC,
                schedule.RecurBetweenEndUTC);
            await InsertJobCoreAsync(schedule.ID, newQueueDate).ConfigureAwait(false);
        }

        Job[] jobs = await ((IJobManager)this).GetLatestJobsAsync(
            pageNumber: 1,
            rowsPerPage: 999,
            statusCode: "RUN",
            workerID: null,
            workerName: null,
            overdueOnly: false);

        return jobs.Length;
    }

    private async Task<JobWithWorker> GetJobWithWorkerAsync(long id)
    {
        (JobEntity[] jobEntities, WorkerEntity[] workerEntities) =
            await GetJobWithWorkerCoreAsync(id).ConfigureAwait(false);

        return ModelBuilders.GetJobWithWorker(jobEntities[0], workerEntities[0]);
    }

    private async Task SendEmailAsync(JobWithWorker jobWithWorker, string adminEmail, string appUrl,
        string environmentName, bool success, string? detailedMessage)
    {
        try
        {
            HashSet<string> toAddresses = new();
            if (!string.IsNullOrWhiteSpace(jobWithWorker.Worker.EmailOnSuccess))
            {
                // Always send to the EmailOnSuccess group, for successes and failures
                foreach (string addr in (jobWithWorker.Worker.EmailOnSuccess ?? "").Split(';').Where(x => !string.IsNullOrWhiteSpace(x)))
                {
                    toAddresses.Add(addr);
                }
            }

            if (!success)
            {
                // For failures, send to the admin
                foreach (string addr in adminEmail.Split(';').Where(x => !string.IsNullOrWhiteSpace(x)))
                {
                    toAddresses.Add(addr);
                }
            }

            if (!toAddresses.Any())
            {
                return;
            }

            string subject = $"[{environmentName}] {(success ? "SUCCESS" : "ERROR")} - Worker: [{jobWithWorker.Worker.WorkerName}]";
            detailedMessage = (detailedMessage ?? "").Replace("\r\n", "<br>").Replace("\r", "<br>").Replace("\n", "<br>");
            string body = $"Job ID: {jobWithWorker.ID}<br><br>{detailedMessage}";

            while (!appUrl.EndsWith("/"))
            {
                appUrl = $"{appUrl}/";
            }
            appUrl += $"Jobs/AcknowledgeError/{jobWithWorker.AcknowledgementCode:N}";
            if (!success)
            {
                body = $"<a href='{appUrl}' target=_blank>Acknowledge error</a><br><br>{body}";
            }

            await _emailer.SendEmailAsync(toAddresses.ToArray(), subject, body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email");
        }
    }
}
