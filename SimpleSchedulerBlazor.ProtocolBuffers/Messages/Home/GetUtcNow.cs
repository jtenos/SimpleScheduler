namespace SimpleSchedulerBlazor.ProtocolBuffers.Messages.Home;

partial class GetUtcNowRequest
{
}

partial class GetUtcNowReply
{
    public GetUtcNowReply(string formattedDateTime)
    {
        FormattedDateTime = formattedDateTime;
    }
}
