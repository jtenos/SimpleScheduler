namespace SimpleSchedulerApiModels.Request.Schedules;

public class DeleteScheduleRequest
{
    public DeleteScheduleRequest() { }

    public DeleteScheduleRequest(long id)
    {
        ID = id;
    }

    public long ID { get; set; }
}
