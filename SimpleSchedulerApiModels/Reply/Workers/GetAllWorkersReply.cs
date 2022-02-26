using System.Runtime.Serialization;

namespace SimpleSchedulerApiModels.Reply.Workers;

[DataContract]
public class GetAllWorkersReply
{
    public GetAllWorkersReply() { }

    public GetAllWorkersReply(WorkerWithSchedules[] workers)
    {
        Workers = workers;
    }

    [DataMember(Order = 1)] public WorkerWithSchedules[] Workers { get; set; } = default!;
}
