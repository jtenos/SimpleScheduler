namespace SimpleSchedulerApiModels.Request.Jobs;

public record class AcknowledgeErrorRequest(
    Guid AcknowledgementCode
);
