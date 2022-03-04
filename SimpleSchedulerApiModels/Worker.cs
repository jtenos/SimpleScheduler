namespace SimpleSchedulerApiModels;

public class Worker
{
    public Worker() { }

    public Worker(long id, bool isActive, string workerName, string detailedDescription,
        string emailOnSuccess, long? parentWorkerID, int timeoutMinutes,
        string directoryName, string executable, string argumentValues)
    {
        ID = id;
        IsActive = isActive;
        WorkerName = workerName;
        DetailedDescription = detailedDescription;
        EmailOnSuccess = emailOnSuccess;
        ParentWorkerID = parentWorkerID;
        TimeoutMinutes = timeoutMinutes;
        DirectoryName = directoryName;
        Executable = executable;
        ArgumentValues = argumentValues;
    }

    public long ID { get; set; }
    public bool IsActive { get; set; }
    public string WorkerName { get; set; } = default!;
    public string DetailedDescription { get; set; } = default!;
    public string EmailOnSuccess { get; set; } = default!;
    public long? ParentWorkerID { get; set; }
    public int TimeoutMinutes { get; set; }
    public string DirectoryName { get; set; } = default!;
    public string Executable { get; set; } = default!;
    public string ArgumentValues { get; set; } = default!;
}