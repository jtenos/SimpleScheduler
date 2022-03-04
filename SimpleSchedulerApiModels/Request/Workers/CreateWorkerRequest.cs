﻿namespace SimpleSchedulerApiModels.Request.Workers;

public class CreateWorkerRequest
{
    public CreateWorkerRequest() { }

    public CreateWorkerRequest(
        string workerName,
        string detailedDescription,
        string emailOnSuccess,
        long? parentWorkerID,
        int timeoutMinutes,
        string directoryName,
        string executable,
        string argumentValues)
    {
        WorkerName = workerName;
        DetailedDescription = detailedDescription;
        EmailOnSuccess = emailOnSuccess;
        ParentWorkerID = parentWorkerID;
        TimeoutMinutes = timeoutMinutes;
        DirectoryName = directoryName;
        Executable = executable;
        ArgumentValues = argumentValues;
    }

    public string WorkerName { get; set; } = default!;
    public string DetailedDescription { get; set; } = default!;
    public string EmailOnSuccess { get; set; } = default!;
    public long? ParentWorkerID { get; set; }
    public int TimeoutMinutes { get; set; }
    public string DirectoryName { get; set; } = default!;
    public string Executable { get; set; } = default!;
    public string ArgumentValues { get; set; } = default!;
}