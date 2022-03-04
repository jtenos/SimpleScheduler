namespace SimpleSchedulerApiModels.Request.Workers;

public class GetWorkerRequest
{
    public GetWorkerRequest() { }

    public GetWorkerRequest(long id)
    {
        ID = id;
    }

    public long ID { get; set; }
}
