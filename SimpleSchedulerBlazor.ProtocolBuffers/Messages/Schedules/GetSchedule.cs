using SimpleSchedulerModels;

namespace SimpleSchedulerBlazor.ProtocolBuffers.Messages.Schedules;

partial class GetScheduleRequest
{
    public GetScheduleRequest(long id)
    {
        ID = id;
    }
}

partial class GetScheduleReply
{
    public GetScheduleReply(Schedule schedule)
    {
        Schedule = new ScheduleMessage(schedule);
    }
}
