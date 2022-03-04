namespace SimpleSchedulerApiModels.Reply.Workers;

public class GetAllWorkersReply
{
    public GetAllWorkersReply() { }

    public GetAllWorkersReply(WorkerWithSchedules[] workers)
    {
        Workers = workers;
    }

    public WorkerWithSchedules[] Workers { get; set; } = default!;
}
