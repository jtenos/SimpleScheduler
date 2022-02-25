using System.Runtime.Serialization;

namespace SimpleSchedulerApiModels.Reply.Schedules;

[DataContract]
public class GetAllSchedulesReply
{
    public GetAllSchedulesReply() { }

    public GetAllSchedulesReply(Schedule[] schedules)
    {
        Schedules = schedules;
    }

    [DataMember(Order = 1)] public Schedule[] Schedules { get; set; } = default!;
}
