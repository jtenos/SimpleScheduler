using System.Runtime.Serialization;

namespace SimpleSchedulerApiModels.Request.Workers;

[DataContract]
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

    [DataMember(Order = 1)] public string WorkerName { get; set; } = default!;
    [DataMember(Order = 2)] public string DetailedDescription { get; set; } = default!;
    [DataMember(Order = 3)] public string EmailOnSuccess { get; set; } = default!;
    [DataMember(Order = 4)] public long? ParentWorkerID { get; set; }
    [DataMember(Order = 5)] public int TimeoutMinutes { get; set; }
    [DataMember(Order = 6)] public string DirectoryName { get; set; } = default!;
    [DataMember(Order = 7)] public string Executable { get; set; } = default!;
    [DataMember(Order = 8)] public string ArgumentValues { get; set; } = default!;
}
