namespace SimpleSchedulerApiModels.Request.Workers;

public record class GetWorkersRequest(
    long[] IDs
);
