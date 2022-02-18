namespace SimpleSchedulerModels;

public record class Worker(long ID, bool IsActive, string WorkerName, string DetailedDescription,
    string EmailOnSuccess, long? ParentWorkerID, long TimeoutMinutes,
    string DirectoryName, string Executable, string ArgumentValues);
