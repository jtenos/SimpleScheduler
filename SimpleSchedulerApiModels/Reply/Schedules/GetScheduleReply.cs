namespace SimpleSchedulerApiModels.Reply.Schedules;

public class GetScheduleReply
{
    public GetScheduleReply() { }

    public GetScheduleReply(Schedule schedule)
    {
        Schedule = schedule;
    }

    public Schedule Schedule { get; set; } = default!;
}
