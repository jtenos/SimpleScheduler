using System.Runtime.Serialization;

namespace SimpleSchedulerApiModels.Reply.Home;

[DataContract]
public class GetUtcNowReply
{
    public GetUtcNowReply() { }

    public GetUtcNowReply(string formattedDateTime)
    {
        FormattedDateTime = formattedDateTime;
    }

    [DataMember(Order = 1)] public string FormattedDateTime { get; set; } = default!;
}
