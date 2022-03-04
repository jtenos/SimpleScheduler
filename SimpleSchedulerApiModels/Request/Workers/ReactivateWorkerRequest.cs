namespace SimpleSchedulerApiModels.Request.Workers;

public class ReactivateWorkerRequest
{
    public ReactivateWorkerRequest() { }

    public ReactivateWorkerRequest(long id)
    {
        ID = id;
    }

    public long ID { get; set; }
}
