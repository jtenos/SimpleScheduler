using SimpleSchedulerModels;

namespace SimpleSchedulerBlazor.ProtocolBuffers.Messages.Schedules;

partial class GetAllSchedulesRequest
{
}

partial class GetAllSchedulesReply
{
    public GetAllSchedulesReply(IEnumerable<Schedule> schedules)
    {
        foreach (Schedule schedule in schedules)
        {
            Schedules.Add(new ScheduleMessage(schedule));
        }
    }
}
