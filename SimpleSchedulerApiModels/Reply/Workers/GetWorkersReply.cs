namespace SimpleSchedulerApiModels.Reply.Workers;

public class GetWorkersReply
{
    public GetWorkersReply() { }

    public GetWorkersReply(Worker[] workers)
    {
        Workers = workers;
    }

    public Worker[] Workers { get; set; } = default!;
}
