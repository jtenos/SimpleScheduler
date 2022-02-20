namespace SimpleSchedulerModels.ApiModels.Workers;

public record class GetAllWorkersRequest();
public record class GetAllWorkersResponse(ImmutableArray<Worker> Workers);
