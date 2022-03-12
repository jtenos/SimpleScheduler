namespace SimpleSchedulerApiModels.Request.Workers;

public record class CreateWorkerRequest(
    string WorkerName,
    string DetailedDescription,
    string EmailOnSuccess,
    long? ParentWorkerID,
    int TimeoutMinutes,
    string DirectoryName,
    string Executable,
    string ArgumentValues
);
