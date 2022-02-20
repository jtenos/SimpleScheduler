namespace SimpleSchedulerModels.ApiModels.Workers;

public record class GetWorkerRequest(long ID);
public record class GetWorkerResponse(Worker Worker);
