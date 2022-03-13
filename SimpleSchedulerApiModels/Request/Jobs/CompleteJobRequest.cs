namespace SimpleSchedulerApiModels.Request.Jobs;

public record class CompleteJobRequest(
    long ID,
    bool Success,
    string? DetailedMessage
);
