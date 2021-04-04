using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using SimpleSchedulerEntities;
using SimpleSchedulerModels;

namespace SimpleSchedulerBusiness
{
    public interface IJobManager
    {
        Task RestartStuckJobsAsync(CancellationToken cancellationToken);
        Task AcknowledgeErrorAsync(long jobID, CancellationToken cancellationToken);
        Task AddJobAsync(long scheduleID, DateTime queueDateUTC, CancellationToken cancellationToken);
        Task<Job> GetJobAsync(long jobID, CancellationToken cancellationToken);
        Task CancelJobAsync(long jobID, CancellationToken cancellationToken);
        Task CompleteJobAsync(long jobID, string statusCode, string? detailedMessage, CancellationToken cancellationToken);
        Task<ImmutableArray<JobDetail>> GetLatestJobsAsync(int pageNumber, int rowsPerPage, 
            string? statusCode, long? workerID, bool overdueOnly, CancellationToken cancellationToken);
        Task<ImmutableArray<JobDetail>> GetOverdueJobsAsync(CancellationToken cancellationToken);
        Task<Job?> GetLastQueuedJobAsync(long scheduleID, CancellationToken cancellationToken);
        Task<string?> GetJobDetailedMessageAsync(long jobID, CancellationToken cancellationToken);
        Task<ImmutableArray<JobDetail>> DequeueScheduledJobsAsync(CancellationToken cancellationToken);
        Job ConvertToJob(JobEntity entity);
    }
}
