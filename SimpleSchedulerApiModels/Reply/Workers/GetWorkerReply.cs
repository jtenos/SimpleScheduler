namespace SimpleSchedulerApiModels.Reply.Workers;

public class GetWorkerReply
{
    public GetWorkerReply() { }

    public GetWorkerReply(WorkerWithSchedules worker)
    {
        Worker = worker;
    }

    public Worker Worker { get; set; } = default!;
    [DataMember(Order = 1)] public WorkerWithSchedules Worker { get; set; } = default!;
}
