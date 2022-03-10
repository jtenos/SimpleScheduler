namespace SimpleSchedulerApiModels.Request.Workers;

public class GetWorkersRequest
{
    public GetWorkersRequest() { }

    public GetWorkersRequest(long[] ids)
    {
        IDs = ids;
    }

    public long[] IDs { get; set; } = default!;
}
