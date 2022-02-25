using System.Runtime.Serialization;

namespace SimpleSchedulerApiModels.Reply.Workers;

[DataContract]
public class GetAllWorkersReply
{
    public GetAllWorkersReply() { }

    public GetAllWorkersReply(Worker[] workers)
    {
        Workers = workers;
    }

    [DataMember(Order = 1)] public Worker[] Workers { get; set; } = default!;
}
