using System.Collections.Immutable;
using Dapper;
using OneOf;
using OneOf.Types;
using SimpleSchedulerAppServices.Interfaces;
using SimpleSchedulerData;
using SimpleSchedulerModels;
using SimpleSchedulerModels.ResultTypes;

namespace SimpleSchedulerAppServices.Implementations.SqlServer;

public sealed class JobManager
    : IJobManager
{
    private readonly SqlDatabase _db;

    public JobManager(SqlDatabase db)
    {
        _db = db;
    }

    async Task IJobManager.AcknowledgeErrorAsync(long jobID, CancellationToken cancellationToken)
    {
        DynamicParameters param = new DynamicParameters()
            .AddLongParam("@ID", jobID);

        await _db.NonQueryAsync(
            "[app].[Jobs_AcknowledgeError]",
            param,
            cancellationToken: cancellationToken
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
            cancellationToken: cancellationToken
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
            cancellationToken: cancellationToken
        ).ConfigureAwait(false);

        if (cancelResult.Success) { return new Success(); }
        if (cancelResult.AlreadyCompleted) { return new AlreadyCompleted(); }
        if (cancelResult.AlreadyStarted) { return new AlreadyStarted(); }

        throw new ApplicationException("Invalid cancel result");
    }

    async Task IJobManager.CompleteJobAsync(long jobID, bool success, string? detailedMessage, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(detailedMessage))
        {
            throw new NotImplementedException("Write the detailed message to disk");
        }

        DynamicParameters param = new DynamicParameters()
            .AddLongParam("@ID", jobID)
            .AddBitParam("@Success", success)
            .AddBitParam("@HasDetailedMessage", !string.IsNullOrWhiteSpace(detailedMessage));

        await _db.NonQueryAsync(
            "[app].[Jobs_Complete]",
            param,
            cancellationToken: cancellationToken
        ).ConfigureAwait(false);
    }

    async Task<ImmutableArray<JobDetail>> IJobManager.DequeueScheduledJobsAsync(CancellationToken cancellationToken)
    {
        (ImmutableArray<Job> jobs, ImmutableArray<Schedule> schedules, ImmutableArray<Worker> workers)
            = await _db.GetManyAsync<Job, Schedule, Worker>(
            "[app].[Jobs_Dequeue]",
            cancellationToken: cancellationToken
        ).ConfigureAwait(false);

        List<JobDetail> jobDetails = new();
        foreach (Job job in jobs)
        {
            Schedule schedule = schedules.Single(s => s.ID == job.ScheduleID);
            Worker worker = workers.Single(w => w.ID == schedule.WorkerID);
            jobDetails.Add(new(job, schedule, worker));
        }

        return jobDetails.ToImmutableArray();
    }

    async Task<Job> IJobManager.GetJobAsync(long jobID, CancellationToken cancellationToken)
    {
        DynamicParameters param = new DynamicParameters()
            .AddLongParam("@ID", jobID);

        return await _db.GetOneAsync<Job>(
            "[app].[Jobs_Select]",
            param,
            cancellationToken: cancellationToken
        ).ConfigureAwait(false);
    }

    async Task<string?> IJobManager.GetDetailedMessageAsync(long jobID, CancellationToken cancellationToken)
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
            cancellationToken: cancellationToken
        ).ConfigureAwait(false);
    }

    async Task<ImmutableArray<JobDetail>> IJobManager.GetLatestJobsAsync(int pageNumber,
        int rowsPerPage, string? statusCode, long? workerID, bool overdueOnly,
        CancellationToken cancellationToken)
    {
        DynamicParameters param = new DynamicParameters()
            .AddNullableNVarCharParam("@StatusCode", statusCode, 3)
            .AddNullableLongParam("@WorkerID", workerID)
            .AddBitParam("@OverdueOnly", overdueOnly)
            .AddIntParam("@Offset", (pageNumber - 1) * rowsPerPage)
            .AddIntParam("@NumRows", rowsPerPage);


        (ImmutableArray<Job> jobs, ImmutableArray<Schedule> schedules, ImmutableArray<Worker> workers)
            = await _db.GetManyAsync<Job, Schedule, Worker>(
                "[app].[Jobs_Search]",
                param,
                cancellationToken: cancellationToken
        ).ConfigureAwait(false);

        List<JobDetail> jobDetails = new();
        foreach (Job job in jobs)
        {
            Schedule schedule = schedules.Single(s => s.ID == job.ScheduleID);
            Worker worker = workers.Single(w => w.ID == schedule.WorkerID);
            jobDetails.Add(new(job, schedule, worker));
        }

        List<JobDetail> filteredJobs;
        if (overdueOnly)
        {
            // For overdue only, filter RUN/ERR/NEW status, and different levels based on the status
            filteredJobs = new();
            foreach (JobDetail jobDetail in jobDetails)
            {
                if (jobDetail.Job.JobStatus == JobStatus.Running)
                {
                    // Greater than the timeout period for the worker
                    if (jobDetail.Job.QueueDateUTC.AddMinutes(jobDetail.Worker.TimeoutMinutes) < DateTime.UtcNow)
                    {
                        filteredJobs.Add(jobDetail);
                    }
                }
                else if (jobDetail.Job.JobStatus == JobStatus.Error)
                {
                    // All errors show up here
                    filteredJobs.Add(jobDetail);
                }
                else if (jobDetail.Job.JobStatus == JobStatus.New)
                {
                    // Been stuck in NEW for more than one minute
                    if (jobDetail.Job.QueueDateUTC.AddMinutes(1) < DateTime.UtcNow)
                    {
                        filteredJobs.Add(jobDetail);
                    }
                }
            }
        }
        else
        {
            filteredJobs = jobDetails;
        }

        return filteredJobs.ToImmutableArray();
    }

    async Task<ImmutableArray<JobDetail>> IJobManager.GetOverdueJobsAsync(CancellationToken cancellationToken)
    {
        return (await ((IJobManager)this).GetLatestJobsAsync(
            pageNumber: 1,
            rowsPerPage: 999,
            statusCode: null,
            workerID: null,
            overdueOnly: true,
            cancellationToken: cancellationToken
        ).ConfigureAwait(false))
        .OrderBy(x => x.Job.QueueDateUTC)
        .ToImmutableArray();
    }

    async Task IJobManager.RestartStuckJobsAsync(CancellationToken cancellationToken)
    {
        await _db.NonQueryAsync(
            "[dbo].[Jobs_RestartStuck]", 
            cancellationToken: cancellationToken
        ).ConfigureAwait(false);
    }
}
