namespace SimpleSchedulerApiModels.Reply.Workers;

public class GetAllActiveWorkerIDNamesReply
{
    public GetAllActiveWorkerIDNamesReply() { }

    public GetAllActiveWorkerIDNamesReply(WorkerIDName[] workers)
    {
        Workers = workers;
    }

    public WorkerIDName[] Workers { get; set; } = Array.Empty<WorkerIDName>();
}
