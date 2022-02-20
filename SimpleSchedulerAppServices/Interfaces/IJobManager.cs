using System.Collections.Immutable;
using OneOf;
using OneOf.Types;
using SimpleSchedulerModels;
using SimpleSchedulerModels.ResultTypes;

namespace SimpleSchedulerAppServices.Interfaces;

public interface IJobManager
{
    Task RestartStuckJobsAsync(CancellationToken cancellationToken);
    Task AcknowledgeErrorAsync(long id, CancellationToken cancellationToken);
    Task AddJobAsync(long scheduleID, DateTime queueDateUTC, CancellationToken cancellationToken);
    Task<Job> GetJobAsync(long id, CancellationToken cancellationToken);
    Task<OneOf<Success, AlreadyCompleted, AlreadyStarted>> CancelJobAsync(long jobID, CancellationToken cancellationToken);
    Task CompleteJobAsync(long id, bool success, string? detailedMessage, CancellationToken cancellationToken);
    Task<ImmutableArray<Job>> GetLatestJobsAsync(int pageNumber, int rowsPerPage,
        string? statusCode, long? workerID, bool overdueOnly, CancellationToken cancellationToken);
    Task<ImmutableArray<Job>> GetOverdueJobsAsync(CancellationToken cancellationToken);
    Task<Job?> GetLastQueuedJobAsync(long scheduleID, CancellationToken cancellationToken);
    Task<string?> GetDetailedMessageAsync(long id, CancellationToken cancellationToken);
    Task<ImmutableArray<Job>> DequeueScheduledJobsAsync(CancellationToken cancellationToken);
}
