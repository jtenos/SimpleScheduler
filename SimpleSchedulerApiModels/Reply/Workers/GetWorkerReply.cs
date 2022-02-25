using System.Runtime.Serialization;

namespace SimpleSchedulerApiModels.Reply.Workers;

[DataContract]
public class GetWorkerReply
{
    public GetWorkerReply() { }

    public GetWorkerReply(Worker worker)
    {
        Worker = worker;
    }

    [DataMember(Order = 1)] public Worker Worker { get; set; } = default!;
}
