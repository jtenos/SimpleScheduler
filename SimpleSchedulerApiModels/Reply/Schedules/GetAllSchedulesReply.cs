namespace SimpleSchedulerApiModels.Reply.Schedules;

public class GetAllSchedulesReply
{
    public GetAllSchedulesReply() { }

    public GetAllSchedulesReply(Schedule[] schedules)
    {
        Schedules = schedules;
    }

    public Schedule[] Schedules { get; set; } = default!;
}
