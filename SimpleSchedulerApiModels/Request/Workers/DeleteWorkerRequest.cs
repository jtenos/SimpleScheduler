namespace SimpleSchedulerApiModels.Request.Workers;

public class DeleteWorkerRequest
{
    public DeleteWorkerRequest()
    {
    }

    public DeleteWorkerRequest(long id)
    {
        ID = id;
    }

    public long ID { get; set; }
}
