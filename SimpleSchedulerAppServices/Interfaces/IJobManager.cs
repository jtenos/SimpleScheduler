using OneOf;
using OneOf.Types;
using SimpleSchedulerModels;
using SimpleSchedulerModels.ResultTypes;

namespace SimpleSchedulerAppServices.Interfaces;

public interface IJobManager
{
    Task RestartStuckJobsAsync();
    Task AcknowledgeErrorAsync(long id);
    Task AddJobAsync(long scheduleID, DateTime queueDateUTC);
    Task<Job> GetJobAsync(long id);
    Task<OneOf<Success, AlreadyCompleted, AlreadyStarted>> CancelJobAsync(long jobID);
    Task CompleteJobAsync(long id, bool success, string? detailedMessage);
    Task<Job[]> GetLatestJobsAsync(int pageNumber, int rowsPerPage,
        string? statusCode, long? workerID, bool overdueOnly);
    Task<Job[]> GetOverdueJobsAsync();
    Task<Job?> GetLastQueuedJobAsync(long scheduleID);
    Task<string?> GetDetailedMessageAsync(long id);
    Task<Job[]> DequeueScheduledJobsAsync();
}
