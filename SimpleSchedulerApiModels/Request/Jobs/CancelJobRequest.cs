namespace SimpleSchedulerApiModels.Request.Jobs;

public class CancelJobRequest
{
    public CancelJobRequest() { }

    public CancelJobRequest(long id)
    {
        ID = id;
    }

    public long ID { get; set; }
}
