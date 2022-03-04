namespace SimpleSchedulerApiModels.Reply.Jobs;

public class GetDetailedMessageReply
{
    public GetDetailedMessageReply() { }

    public GetDetailedMessageReply(string? detailedMessage)
    {
        DetailedMessage = detailedMessage;
    }

    public string? DetailedMessage { get; set; }
}
