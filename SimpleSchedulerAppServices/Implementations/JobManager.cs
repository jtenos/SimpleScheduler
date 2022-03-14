using System.Data;
using System.Text;
using Dapper;
using Microsoft.Extensions.Logging;
using SimpleSchedulerAppServices.Interfaces;
using SimpleSchedulerAppServices.Utilities;
using SimpleSchedulerData;
using SimpleSchedulerDataEntities;
using SimpleSchedulerDomainModels;
using SimpleSchedulerEmail;

namespace SimpleSchedulerAppServices.Implementations;

public sealed class JobManager
    : IJobManager
{
    private readonly ILogger<JobManager> _logger;
    private readonly SqlDatabase _db;
    private readonly IEmailer _emailer;

    public JobManager(ILogger<JobManager> logger, SqlDatabase db, IEmailer emailer)
    {
        _logger = logger;
        _db = db;
        _emailer = emailer;
    }

    async Task IJobManager.AcknowledgeErrorAsync(Guid acknowledgementCode)
    {
        DynamicParameters param = new DynamicParameters()
            .AddUniqueIdentifierParam("@AcknowledgementCode", acknowledgementCode);

        await _db.NonQueryAsync(
            "[app].[Jobs_AcknowledgeError]",
            param
        ).ConfigureAwait(false);
    }

    private record class CancelJobResult(bool Success, bool AlreadyCompleted, bool AlreadyStarted);
    async Task IJobManager.CancelJobAsync(long jobID)
    {
        DynamicParameters param = new DynamicParameters()
            .AddLongParam("@ID", jobID);

        CancelJobResult cancelResult = await _db.GetOneAsync<CancelJobResult>(
            "[app].[Jobs_Cancel]",
            param
        ).ConfigureAwait(false);

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
            FileInfo messageGZipFile = new(Path.Combine(messageDir.FullName, $"{id}.txt.gz"));

            GZipTextFile(messageGZipFile, detailedMessage.Trim());
        }

        DynamicParameters param = new DynamicParameters()
            .AddLongParam("@ID", id)
            .AddBitParam("@Success", success)
            .AddBitParam("@HasDetailedMessage", !string.IsNullOrWhiteSpace(detailedMessage));

        await _db.NonQueryAsync(
            "[app].[Jobs_Complete]",
            param
        ).ConfigureAwait(false);

        JobWithWorker jobWithWorker = await GetJobWithWorkerAsync(id);

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
        (JobWithWorkerIDEntity[] jobEntities, WorkerEntity[] workerEntities) = await _db.GetManyAsync<JobWithWorkerIDEntity, WorkerEntity>(
            "[app].[Jobs_Dequeue]",
            parameters: null
        ).ConfigureAwait(false);

        List<JobWithWorker> result = new();
        foreach (JobWithWorkerIDEntity jobEntity in jobEntities)
        {
            WorkerEntity workerEntity = workerEntities.Single(w => w.ID == jobEntity.WorkerID);
            result.Add(ModelBuilders.GetJobWithWorker(jobEntity, workerEntity));
        }
        return result.ToArray();
    }

    async Task<Job> IJobManager.GetJobAsync(long id)
    {
        DynamicParameters param = new DynamicParameters()
            .AddLongParam("@ID", id);

        return ModelBuilders.GetJob(await _db.GetOneAsync<JobEntity>(
            "[app].[Jobs_Select]",
            param
        ).ConfigureAwait(false));
    }

    Task<string> IJobManager.GetDetailedMessageAsync(long id, string workerPath)
    {
        DirectoryInfo messageDir = new(Path.Combine(workerPath, "__messages__"));
        if (!messageDir.Exists)
        {
            return Task.FromResult("** NO MESSAGE FILE FOUND **");
        }
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
        int rowsPerPage, string? statusCode, long? workerID, bool overdueOnly)
    {
        DynamicParameters param = new DynamicParameters()
            .AddNullableNCharParam("@StatusCode", statusCode, 3)
            .AddNullableLongParam("@WorkerID", workerID)
            .AddBitParam("@OverdueOnly", overdueOnly)
            .AddIntParam("@Offset", (pageNumber - 1) * rowsPerPage)
            .AddIntParam("@NumRows", rowsPerPage);

        JobWithWorkerIDEntity[] jobs = await _db.GetManyAsync<JobWithWorkerIDEntity>(
            "[app].[Jobs_Search]",
            param
        ).ConfigureAwait(false);

        if (!overdueOnly)
        {
            return jobs.Select(j => ModelBuilders.GetJobWithWorkerID(j)).ToArray();
        }

        // For overdue only, filter RUN/ERR/NEW status, and different levels based on the status
        List<JobWithWorkerIDEntity> filteredJobs;
        filteredJobs = new();

        param = new DynamicParameters()
            .AddBigIntArrayParam("@IDs", jobs.Select(j => j.ScheduleID).Distinct().ToArray());

        Dictionary<long, ScheduleEntity> schedules = (await _db.GetManyAsync<ScheduleEntity>(
            "[app].[Schedules_SelectMany]",
            parameters: param
        ).ConfigureAwait(false))
        .ToDictionary(s => s.ID, s => s);

        param = new DynamicParameters()
            .AddBigIntArrayParam("@IDs", schedules.Select(s => s.Value.WorkerID).Distinct().ToArray());

        Dictionary<long, WorkerEntity> workers = (await _db.GetManyAsync<WorkerEntity>(
            "[app].[Workers_SelectMany]",
            parameters: param
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
            overdueOnly: true
        ).ConfigureAwait(false))
        .OrderBy(x => x.QueueDateUTC)
        .ToArray();
    }

    async Task IJobManager.RestartStuckJobsAsync()
    {
        await _db.NonQueryAsync(
            "[app].[Jobs_RestartStuck]",
            parameters: null
        ).ConfigureAwait(false);
    }

    async Task<int> IJobManager.StartDueJobsAsync()
    {
        Schedule[] schedulesToInsert = (await _db.GetManyAsync<ScheduleEntity>(
            "[app].[Schedules_SelectForJobInsertion]",
            parameters: null
        ).ConfigureAwait(false))
        .Select(s => ModelBuilders.GetSchedule(s))
        .ToArray();

        foreach (Schedule schedule in schedulesToInsert)
        {
            _logger.LogInformation("Inserting job for schedule {scheduleID} (Worker {workerID})",
                schedule.ID, schedule.WorkerID);
            var lastQueuedJob = await GetLastQueuedJobAsync(schedule.ID).ConfigureAwait(false);
            DateTime? lastQueueDate = lastQueuedJob?.QueueDateUTC;

            var daysOfTheWeek = new List<DayOfWeek>();
            if (schedule.Sunday) daysOfTheWeek.Add(DayOfWeek.Sunday);
            if (schedule.Monday) daysOfTheWeek.Add(DayOfWeek.Monday);
            if (schedule.Tuesday) daysOfTheWeek.Add(DayOfWeek.Tuesday);
            if (schedule.Wednesday) daysOfTheWeek.Add(DayOfWeek.Wednesday);
            if (schedule.Thursday) daysOfTheWeek.Add(DayOfWeek.Thursday);
            if (schedule.Friday) daysOfTheWeek.Add(DayOfWeek.Friday);
            if (schedule.Saturday) daysOfTheWeek.Add(DayOfWeek.Saturday);

            var newQueueDate = ScheduleFinder.GetNextDate(lastQueueDate, daysOfTheWeek.ToArray(),
                schedule.TimeOfDayUTC,
                schedule.RecurTime,
                schedule.RecurBetweenStartUTC,
                schedule.RecurBetweenEndUTC);
            await AddJobAsync(schedule.ID, newQueueDate).ConfigureAwait(false);
        }

        Job[] jobs = await ((IJobManager)this).GetLatestJobsAsync(
            pageNumber: 1,
            rowsPerPage: 999,
            statusCode: "RUN",
            workerID: null,
            overdueOnly: false);

        return jobs.Length;
    }

    private async Task AddJobAsync(long scheduleID, DateTime queueDateUTC)
    {
        DynamicParameters param = new DynamicParameters()
            .AddLongParam("@ScheduleID", scheduleID)
            .AddDateTimeParam("@QueueDateUTC", queueDateUTC);

        await _db.NonQueryAsync(
            "[app].[Jobs_Insert]",
            param
        ).ConfigureAwait(false);
    }

    private async Task<Job?> GetLastQueuedJobAsync(long scheduleID)
    {
        DynamicParameters param = new DynamicParameters()
            .AddLongParam("@ScheduleID", scheduleID);

        JobEntity? jobEntity = await _db.GetZeroOrOneAsync<JobEntity>(
            "[app].[Jobs_SelectMostRecentBySchedule]",
            param
        ).ConfigureAwait(false);
        if (jobEntity is null)
        {
            return null;
        }
        return ModelBuilders.GetJob(jobEntity);
    }

    private async Task<JobWithWorker> GetJobWithWorkerAsync(long id)
    {
        DynamicParameters param = new DynamicParameters()
            .AddLongParam("@ID", id);
        (JobEntity[] jobEntities, WorkerEntity[] workerEntities) = await _db.GetManyAsync<JobEntity, WorkerEntity>(
            "[app].[JobsWithWorker_Select]",
            parameters: param
        ).ConfigureAwait(false);

        return ModelBuilders.GetJobWithWorker(
            jobEntities[0], workerEntities[0]
        );
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
            appUrl += $"acknowledge-error/{jobWithWorker.AcknowledgementCode:N}";
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
