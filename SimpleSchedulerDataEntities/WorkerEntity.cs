namespace SimpleSchedulerDataEntities;

public record class WorkerEntity(
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
