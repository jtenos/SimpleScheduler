namespace SimpleSchedulerApiModels.Request.Jobs;

public class AcknowledgeErrorRequest
{
    public AcknowledgeErrorRequest() { }

    public AcknowledgeErrorRequest(Guid acknowledgementCode)
    {
        AcknowledgementCode = acknowledgementCode;
    }

    public Guid AcknowledgementCode { get; set; }
}
