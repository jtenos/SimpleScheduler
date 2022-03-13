using System.Data;
using System.Text;
using Dapper;
using SimpleSchedulerAppServices.Interfaces;
using SimpleSchedulerAppServices.Utilities;
using SimpleSchedulerData;
using SimpleSchedulerDataEntities;
using SimpleSchedulerModels;

namespace SimpleSchedulerAppServices.Implementations;

public sealed class JobManager
    : IJobManager
{
    private readonly SqlDatabase _db;

    public JobManager(SqlDatabase db)
    {
        _db = db;
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

    async Task IJobManager.AddJobAsync(long scheduleID, DateTime queueDateUTC)
    {
        DynamicParameters param = new DynamicParameters()
            .AddLongParam("@ScheduleID", scheduleID)
            .AddDateTimeParam("@QueueDateUTC", queueDateUTC);

        await _db.NonQueryAsync(
            "[app].[Jobs_Insert]",
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

    async Task IJobManager.CompleteJobAsync(long id, bool success, string? detailedMessage)
    {
        if (!string.IsNullOrWhiteSpace(detailedMessage))
        {
            throw new NotImplementedException("Write the detailed message to disk");
        }

        DynamicParameters param = new DynamicParameters()
            .AddLongParam("@ID", id)
            .AddBitParam("@Success", success)
            .AddBitParam("@HasDetailedMessage", !string.IsNullOrWhiteSpace(detailedMessage));

        await _db.NonQueryAsync(
            "[app].[Jobs_Complete]",
            param
        ).ConfigureAwait(false);
    }

    async Task<Job[]> IJobManager.DequeueScheduledJobsAsync()
    {
        return (await _db.GetManyAsync<JobEntity>(
            "[app].[Jobs_Dequeue]",
            parameters: null
        ).ConfigureAwait(false))
        .Select(j => ModelBuilders.GetJob(j))
        .ToArray();
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

    async Task<Job?> IJobManager.GetLastQueuedJobAsync(long scheduleID)
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
}
