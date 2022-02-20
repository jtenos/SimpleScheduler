namespace SimpleSchedulerModels.ApiModels.Jobs;

public record class GetJobsRequest(
    long? WorkerID,
    string? StatusCode,
    int PageNumber,
    bool OverdueOnly
);

public record class GetJobsResponse(ImmutableArray<Job> Jobs);
