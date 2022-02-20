using System.Collections.Immutable;
using System.Data;
using Dapper;
using OneOf;
using OneOf.Types;
using SimpleSchedulerAppServices.Interfaces;
using SimpleSchedulerData;
using SimpleSchedulerModels;
using SimpleSchedulerModels.ResultTypes;

namespace SimpleSchedulerAppServices.Implementations;

public sealed class JobManager
    : IJobManager
{
    private readonly SqlDatabase _db;

    public JobManager(SqlDatabase db)
    {
        _db = db;
    }

    async Task IJobManager.AcknowledgeErrorAsync(long id, CancellationToken cancellationToken)
    {
        DynamicParameters param = new DynamicParameters()
            .AddLongParam("@ID", id);

        await _db.NonQueryAsync(
            "[app].[Jobs_AcknowledgeError]",
            param,
            cancellationToken
        ).ConfigureAwait(false);
    }

    async Task IJobManager.AddJobAsync(long scheduleID, DateTime queueDateUTC, CancellationToken cancellationToken)
    {
        DynamicParameters param = new DynamicParameters()
            .AddLongParam("@ScheduleID", scheduleID)
            .AddDateTimeParam("@QueueDateUTC", queueDateUTC);

        await _db.NonQueryAsync(
            "[app].[Jobs_Insert]",
            param,
            cancellationToken
        ).ConfigureAwait(false);
    }

    private record class CancelJobResult(bool Success, bool AlreadyCompleted, bool AlreadyStarted);
    async Task<OneOf<Success, AlreadyCompleted, AlreadyStarted>> IJobManager.CancelJobAsync(long jobID, CancellationToken cancellationToken)
    {
        DynamicParameters param = new DynamicParameters()
            .AddLongParam("@ID", jobID);

        CancelJobResult cancelResult = await _db.GetOneAsync<CancelJobResult>(
            "[app].[Jobs_Cancel]",
            param,
            cancellationToken
        ).ConfigureAwait(false);

        if (cancelResult.Success) { return new Success(); }
        if (cancelResult.AlreadyCompleted) { return new AlreadyCompleted(); }
        if (cancelResult.AlreadyStarted) { return new AlreadyStarted(); }

        throw new ApplicationException("Invalid cancel result");
    }

    async Task IJobManager.CompleteJobAsync(long id, bool success, string? detailedMessage, CancellationToken cancellationToken)
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
            param,
            cancellationToken
        ).ConfigureAwait(false);
    }

    async Task<ImmutableArray<Job>> IJobManager.DequeueScheduledJobsAsync(CancellationToken cancellationToken)
    {
        return await _db.GetManyAsync<Job>(
            "[app].[Jobs_Dequeue]",
            parameters: null,
            cancellationToken
        ).ConfigureAwait(false);
    }

    async Task<Job> IJobManager.GetJobAsync(long id, CancellationToken cancellationToken)
    {
        DynamicParameters param = new DynamicParameters()
            .AddLongParam("@ID", id);

        return await _db.GetOneAsync<Job>(
            "[app].[Jobs_Select]",
            param,
            cancellationToken
        ).ConfigureAwait(false);
    }

    async Task<string?> IJobManager.GetDetailedMessageAsync(long id, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        throw new NotImplementedException("GetDetailedMessageAsync");
    }

    async Task<Job?> IJobManager.GetLastQueuedJobAsync(long scheduleID, CancellationToken cancellationToken)
    {
        DynamicParameters param = new DynamicParameters()
            .AddLongParam("@ScheduleID", scheduleID);

        return await _db.GetZeroOrOneAsync<Job>(
            "[app].[Jobs_SelectMostRecentBySchedule]",
            param,
            cancellationToken
        ).ConfigureAwait(false);
    }

    async Task<ImmutableArray<Job>> IJobManager.GetLatestJobsAsync(int pageNumber,
        int rowsPerPage, string? statusCode, long? workerID, bool overdueOnly,
        CancellationToken cancellationToken)
    {
        DynamicParameters param = new DynamicParameters()
            .AddNullableNVarCharParam("@StatusCode", statusCode, 3)
            .AddNullableLongParam("@WorkerID", workerID)
            .AddBitParam("@OverdueOnly", overdueOnly)
            .AddIntParam("@Offset", (pageNumber - 1) * rowsPerPage)
            .AddIntParam("@NumRows", rowsPerPage);

        ImmutableArray<Job> jobs = await _db.GetManyAsync<Job>(
            "[app].[Jobs_Search]",
            param,
            cancellationToken
        ).ConfigureAwait(false);

        if (!overdueOnly)
        {
            return jobs;
        }

        // For overdue only, filter RUN/ERR/NEW status, and different levels based on the status
        List<Job> filteredJobs;
        filteredJobs = new();

        param = new DynamicParameters()
            .AddBigIntArrayParam("@IDs", jobs.Select(j => j.ScheduleID).Distinct().ToImmutableArray());

        Dictionary<long, Schedule> schedules = (await _db.GetManyAsync<Schedule>(
            "[app].[Schedules_SelectMany]",
            parameters: param,
            cancellationToken: cancellationToken
        ).ConfigureAwait(false))
        .ToDictionary(s => s.ID, s => s);

        param = new DynamicParameters()
            .AddBigIntArrayParam("@IDs", schedules.Select(s => s.Value.WorkerID).Distinct().ToImmutableArray());

        Dictionary<long, Worker> workers = (await _db.GetManyAsync<Worker>(
            "[app].[Workers_SelectMany]",
            parameters: param,
            cancellationToken: cancellationToken
        ).ConfigureAwait(false))
        .ToDictionary(w => w.ID, w => w);

        foreach (Job job in jobs)
        {
            Schedule schedule = schedules[job.ScheduleID];
            Worker worker = workers[schedule.WorkerID];

            if (job.JobStatus == JobStatus.Running)
            {
                // Greater than the timeout period for the worker
                if (job.QueueDateUTC.AddMinutes(worker.TimeoutMinutes) < DateTime.UtcNow)
                {
                    filteredJobs.Add(job);
                }
            }
            else if (job.JobStatus == JobStatus.Error)
            {
                // All errors show up here
                filteredJobs.Add(job);
            }
            else if (job.JobStatus == JobStatus.New)
            {
                // Been stuck in NEW for more than one minute
                if (job.QueueDateUTC.AddMinutes(1) < DateTime.UtcNow)
                {
                    filteredJobs.Add(job);
                }
            }
        }

        return filteredJobs.ToImmutableArray();
    }

    async Task<ImmutableArray<Job>> IJobManager.GetOverdueJobsAsync(CancellationToken cancellationToken)
    {
        return (await ((IJobManager)this).GetLatestJobsAsync(
            pageNumber: 1,
            rowsPerPage: 999,
            statusCode: null,
            workerID: null,
            overdueOnly: true,
            cancellationToken
        ).ConfigureAwait(false))
        .OrderBy(x => x.QueueDateUTC)
        .ToImmutableArray();
    }

    async Task IJobManager.RestartStuckJobsAsync(CancellationToken cancellationToken)
    {
        await _db.NonQueryAsync(
            "[app].[Jobs_RestartStuck]",
            parameters: null,
            cancellationToken
        ).ConfigureAwait(false);
    }
}
