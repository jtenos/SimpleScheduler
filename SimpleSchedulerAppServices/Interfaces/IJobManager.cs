using SimpleSchedulerDomainModels;

namespace SimpleSchedulerAppServices.Interfaces;

public interface IJobManager
{
    Task RestartStuckJobsAsync();
    Task AcknowledgeErrorAsync(Guid acknowledgementCode);
    Task<Job> GetJobAsync(long id);
    Task CancelJobAsync(long jobID);
    Task CompleteJobAsync(long id, bool success, string? detailedMessage,
        string adminEmail, string appUrl, string environmentName, string workerPath);
    Task<JobWithWorkerID[]> GetLatestJobsAsync(int pageNumber, int rowsPerPage,
        string? statusCode, long? workerID, bool overdueOnly);
    Task<Job[]> GetOverdueJobsAsync();
    Task<string> GetDetailedMessageAsync(long id, string workerPath);
    Task<JobWithWorker[]> DequeueScheduledJobsAsync();
    Task<int> StartDueJobsAsync();
}
