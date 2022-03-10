namespace SimpleSchedulerApiModels.Request.Schedules;

public class GetSchedulesRequest
{
    public GetSchedulesRequest() { }

    public GetSchedulesRequest(long[] ids)
    {
        IDs = ids;
    }

    public long[] IDs { get; set; } = default!;
}
