namespace SimpleSchedulerModels.ApiModels.Schedules;

public record class GetScheduleRequest(long ID);
public record class GetScheduleResponse(Schedule Schedule);
