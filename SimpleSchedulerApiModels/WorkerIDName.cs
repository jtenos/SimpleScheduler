using System.Runtime.Serialization;

namespace SimpleSchedulerApiModels;

[DataContract]
public class WorkerIDName
{
    public WorkerIDName() { }

    public WorkerIDName(long id, string workerName)
    {
        ID = id;
        WorkerName = workerName;
    }

    [DataMember(Order = 1)] public long ID { get; set; }
    [DataMember(Order = 2)] public string WorkerName { get; set; } = default!;
}
