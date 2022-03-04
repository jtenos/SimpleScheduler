namespace SimpleSchedulerApiModels;

public class WorkerIDName
{
    public WorkerIDName() { }

    public WorkerIDName(long id, string workerName)
    {
        ID = id;
        WorkerName = workerName;
    }

    public long ID { get; set; }
    public string WorkerName { get; set; } = default!;
}
