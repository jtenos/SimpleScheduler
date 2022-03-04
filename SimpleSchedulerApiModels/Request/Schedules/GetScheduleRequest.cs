namespace SimpleSchedulerApiModels.Request.Schedules;

public class GetScheduleRequest
{
    public GetScheduleRequest() { }

    public GetScheduleRequest(long id)
    {
        ID = id;
    }

    public long ID { get; set; }
}
