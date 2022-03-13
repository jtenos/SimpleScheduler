namespace SimpleSchedulerDomainModels;

public record class Worker(
    long ID,
    bool IsActive,
    string WorkerName,
    string DetailedDescription,
    string EmailOnSuccess,
    long? ParentWorkerID,
    int TimeoutMinutes,
    string DirectoryName,
    string Executable,
    string ArgumentValues
);
