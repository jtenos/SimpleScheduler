namespace SimpleSchedulerApiModels.Request.Workers;

public class RunNowRequest
{
    public RunNowRequest() { }

    public RunNowRequest(long id)
    {
        ID = id;
    }

    public long ID { get; set; }
}
