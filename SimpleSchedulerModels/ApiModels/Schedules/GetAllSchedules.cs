namespace SimpleSchedulerModels.ApiModels.Schedules;

public record class GetAllSchedulesRequest();
public record class GetAllSchedulesResponse(ImmutableArray<Schedule> Schedules);
