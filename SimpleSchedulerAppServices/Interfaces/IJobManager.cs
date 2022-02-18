using System.Collections.Immutable;
using OneOf;
using OneOf.Types;
using SimpleSchedulerModels;
using SimpleSchedulerModels.ResultTypes;

namespace SimpleSchedulerAppServices.Interfaces;

public interface IJobManager
{
    Task RestartStuckJobsAsync(CancellationToken cancellationToken);
    Task AcknowledgeErrorAsync(long jobID, CancellationToken cancellationToken);
    Task AddJobAsync(long scheduleID, DateTime queueDateUTC, CancellationToken cancellationToken);
    Task<Job> GetJobAsync(long jobID, CancellationToken cancellationToken);
    Task<OneOf<Success, AlreadyCompleted, AlreadyStarted>> CancelJobAsync(long jobID, CancellationToken cancellationToken);
    Task CompleteJobAsync(long jobID, bool success, string? detailedMessage, CancellationToken cancellationToken);
    Task<ImmutableArray<JobDetail>> GetLatestJobsAsync(int pageNumber, int rowsPerPage,
        string? statusCode, long? workerID, bool overdueOnly, CancellationToken cancellationToken);
    Task<ImmutableArray<JobDetail>> GetOverdueJobsAsync(CancellationToken cancellationToken);
    Task<Job?> GetLastQueuedJobAsync(long scheduleID, CancellationToken cancellationToken);
    Task<string?> GetDetailedMessageAsync(long jobID, CancellationToken cancellationToken);
    Task<ImmutableArray<JobDetail>> DequeueScheduledJobsAsync(CancellationToken cancellationToken);
}
