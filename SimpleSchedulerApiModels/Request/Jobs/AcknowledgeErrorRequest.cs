namespace SimpleSchedulerApiModels.Request.Jobs;

public class AcknowledgeErrorRequest
{
    public AcknowledgeErrorRequest() { }

    public AcknowledgeErrorRequest(long id)
    {
        ID = id;
    }

    public long ID { get; set; }
}
