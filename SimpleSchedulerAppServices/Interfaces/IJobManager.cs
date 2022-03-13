using SimpleSchedulerModels;

namespace SimpleSchedulerAppServices.Interfaces;

public interface IJobManager
{
    Task RestartStuckJobsAsync();
    Task AcknowledgeErrorAsync(Guid acknowledgementCode);
    Task AddJobAsync(long scheduleID, DateTime queueDateUTC);
    Task<Job> GetJobAsync(long id);
    Task CancelJobAsync(long jobID);
    Task CompleteJobAsync(long id, bool success, string? detailedMessage);
    Task<JobWithWorkerID[]> GetLatestJobsAsync(int pageNumber, int rowsPerPage,
        string? statusCode, long? workerID, bool overdueOnly);
    Task<Job[]> GetOverdueJobsAsync();
    Task<Job?> GetLastQueuedJobAsync(long scheduleID);
    Task<string> GetDetailedMessageAsync(long id, string workerPath);
    Task<Job[]> DequeueScheduledJobsAsync();
}
