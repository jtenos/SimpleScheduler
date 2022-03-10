namespace SimpleSchedulerApiModels.Reply.Schedules;

public class GetSchedulesReply
{
    public GetSchedulesReply() { }

    public GetSchedulesReply(Schedule[] schedules)
    {
        Schedules = schedules;
    }

    public Schedule[] Schedules { get; set; } = default!;
}
