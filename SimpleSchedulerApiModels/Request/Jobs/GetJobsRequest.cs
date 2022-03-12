namespace SimpleSchedulerApiModels.Request.Jobs;

public record class GetJobsRequest(
    long? WorkerID,
    string? StatusCode,
    int PageNumber,
    bool OverdueOnly
);
