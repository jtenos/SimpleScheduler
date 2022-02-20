namespace SimpleSchedulerModels.ApiModels.Jobs;

public record class GetDetailedMessageRequest(long ID);
public record class GetDetailedMessageResponse(string? DetailedMessage);
