using System.Runtime.Serialization;

namespace SimpleSchedulerApiModels.Reply.Jobs;

[DataContract]
public class GetDetailedMessageReply
{
    public GetDetailedMessageReply() { }

    public GetDetailedMessageReply(string? detailedMessage)
    {
        DetailedMessage = detailedMessage;
    }

    [DataMember(Order = 1)] public string? DetailedMessage { get; set; }
}
