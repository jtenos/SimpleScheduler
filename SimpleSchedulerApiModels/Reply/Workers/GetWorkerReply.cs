using System.Runtime.Serialization;

namespace SimpleSchedulerApiModels.Reply.Workers;

[DataContract]
public class GetWorkerReply
{
    public GetWorkerReply() { }

    public GetWorkerReply(WorkerWithSchedules worker)
    {
        Worker = worker;
    }

    [DataMember(Order = 1)] public WorkerWithSchedules Worker { get; set; } = default!;
}
