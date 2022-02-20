namespace SimpleSchedulerModels.ApiModels.Workers;

public record class UpdateWorkerRequest(
    long WorkerID,
    string WorkerName,
    string DetailedDescription,
    string EmailOnSuccess,
    long? ParentWorkerID,
    int TimeoutMinutes,
    string DirectoryName,
    string Executable,
    string ArgumentValues
);
public record class UpdateWorkerResponse();
