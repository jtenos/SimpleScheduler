namespace SimpleSchedulerApiModels.Request.Schedules;

public class ReactivateScheduleRequest
{
    public ReactivateScheduleRequest() { }

    public ReactivateScheduleRequest(long id)
    {
        ID = id;
    }

    public long ID { get; set; }
}
