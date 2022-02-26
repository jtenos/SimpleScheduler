using System.Runtime.Serialization;

namespace SimpleSchedulerApiModels.Reply.Workers;

[DataContract]
public class GetAllActiveWorkerIDNamesReply
{
    public GetAllActiveWorkerIDNamesReply() { }

    public GetAllActiveWorkerIDNamesReply(WorkerIDName[] workers)
    {
        Workers = workers;
    }

    [DataMember(Order = 1)] public WorkerIDName[] Workers { get; set; } = Array.Empty<WorkerIDName>();
}
