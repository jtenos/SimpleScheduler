using System.Runtime.Serialization;

namespace SimpleSchedulerApiModels.Request.Workers;

[DataContract]
public class UpdateWorkerRequest
{
    public UpdateWorkerRequest()
    {
    }

    public UpdateWorkerRequest(
        long id,
        string workerName,
        string detailedDescription,
        string emailOnSuccess,
        long? parentWorkerID,
        int timeoutMinutes,
        string directoryName,
        string executable,
        string argumentValues)
    {
        ID = id;
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
    [DataMember(Order = 2)] public string WorkerName { get; set; } = default!;
    [DataMember(Order = 3)] public string DetailedDescription { get; set; } = default!;
    [DataMember(Order = 4)] public string EmailOnSuccess { get; set; } = default!;
    [DataMember(Order = 5)] public long? ParentWorkerID { get; set; }
    [DataMember(Order = 6)] public int TimeoutMinutes { get; set; }
    [DataMember(Order = 7)] public string DirectoryName { get; set; } = default!;
    [DataMember(Order = 8)] public string Executable { get; set; } = default!;
    [DataMember(Order = 9)] public string ArgumentValues { get; set; } = default!;
}
