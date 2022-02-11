namespace SimpleSchedulerModels;

public record Worker(long WorkerID, bool IsActive, string WorkerName, string DetailedDescription,
    string EmailOnSuccess, long? ParentWorkerID, long TimeoutMinutes,
    string DirectoryName, string Executable, string ArgumentValues);
