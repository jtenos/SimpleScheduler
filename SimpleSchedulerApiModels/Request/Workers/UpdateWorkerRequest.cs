namespace SimpleSchedulerApiModels.Request.Workers;

public record class UpdateWorkerRequest(
    long ID,
    string WorkerName,
    string DetailedDescription,
    string EmailOnSuccess,
    long? ParentWorkerID,
    int TimeoutMinutes,
    string DirectoryName,
    string Executable,
    string ArgumentValues
);
