using SimpleSchedulerBlazor.ProtocolBuffers.Types;

namespace SimpleSchedulerBlazor.ProtocolBuffers.Messages.Jobs;

partial class GetDetailedMessageRequest
{
    public GetDetailedMessageRequest(long id)
    {
        ID = id;
    }
}

partial class GetDetailedMessageReply
{
    public GetDetailedMessageReply(string? detailedMessage)
    {
        DetailedMessage = new NullableString(detailedMessage);
    }
}
