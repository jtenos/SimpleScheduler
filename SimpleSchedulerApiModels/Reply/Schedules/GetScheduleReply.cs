using System.Runtime.Serialization;

namespace SimpleSchedulerApiModels.Reply.Schedules;

[DataContract]
public class GetScheduleReply
{
    public GetScheduleReply() { }

    public GetScheduleReply(Schedule schedule)
    {
        Schedule = schedule;
    }

    [DataMember(Order = 1)] public Schedule Schedule { get; set; } = default!;
}
