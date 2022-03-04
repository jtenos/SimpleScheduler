namespace SimpleSchedulerApiModels.Reply.Workers;

public class GetWorkerReply
{
    public GetWorkerReply() { }

    public GetWorkerReply(Worker worker)
    {
        Worker = worker;
    }

    public Worker Worker { get; set; } = default!;
}
