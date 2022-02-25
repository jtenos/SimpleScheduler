using System.Runtime.Serialization;

namespace SimpleSchedulerApiModels;

[DataContract]
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

    [DataMember(Order = 1)] public long ID { get; set; }
    [DataMember(Order = 2)] public bool IsActive { get; set; }
    [DataMember(Order = 3)] public string WorkerName { get; set; } = default!;
    [DataMember(Order = 4)] public string DetailedDescription { get; set; } = default!;
    [DataMember(Order = 5)] public string EmailOnSuccess { get; set; } = default!;
    [DataMember(Order = 6)] public long? ParentWorkerID { get; set; }
    [DataMember(Order = 7)] public int TimeoutMinutes { get; set; }
    [DataMember(Order = 8)] public string DirectoryName { get; set; } = default!;
    [DataMember(Order = 9)] public string Executable { get; set; } = default!;
    [DataMember(Order = 10)] public string ArgumentValues { get; set; } = default!;
}
