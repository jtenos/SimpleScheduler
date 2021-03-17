using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using SimpleSchedulerModels;

namespace SimpleSchedulerBusiness
{
    public interface IJobManager
    {
        Task RestartStuckJobsAsync(CancellationToken cancellationToken);
        Task AcknowledgeErrorAsync(int jobID, CancellationToken cancellationToken);
        Task AddJobAsync(int scheduleID, DateTime queueDateUTC, CancellationToken cancellationToken);
        Task<Job> GetJobAsync(int jobID, CancellationToken cancellationToken);
        Task CancelJobAsync(int jobID, CancellationToken cancellationToken);
        Task CompleteJobAsync(int jobID, string statusCode, string? detailedMessage, CancellationToken cancellationToken);
        Task<ImmutableArray<JobDetail>> GetLatestJobsAsync(int pageNumber, int rowsPerPage, CancellationToken cancellationToken,
            string? statusCode = null, int? workerID = null, bool overdueOnly = false);
        Task<ImmutableArray<JobDetail>> GetOverdueJobsAsync(CancellationToken cancellationToken);
        Task<Job?> GetLastQueuedJobAsync(int scheduleID, CancellationToken cancellationToken);
        Task<string?> GetJobDetailedMessageAsync(int jobID, CancellationToken cancellationToken);
        Task<ImmutableArray<JobDetail>> DequeueScheduledJobsAsync(CancellationToken cancellationToken);
    }
}
