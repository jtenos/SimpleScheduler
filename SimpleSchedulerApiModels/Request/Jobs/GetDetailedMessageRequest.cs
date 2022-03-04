namespace SimpleSchedulerApiModels.Request.Jobs;

public class GetDetailedMessageRequest
{
    public GetDetailedMessageRequest() { }

    public GetDetailedMessageRequest(long id)
    {
        ID = id;
    }

    public long ID { get; set; }
}
