namespace SimpleSchedulerApiModels.Request.Jobs;

public record class GetJobsRequest(
    long? WorkerID,
    string? WorkerName,
    string? StatusCode,
    int PageNumber,
    bool OverdueOnly
);
