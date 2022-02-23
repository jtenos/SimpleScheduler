namespace SimpleSchedulerBlazor.ProtocolBuffers.Messages.Home;

partial class HelloThereRequest
{
}

partial class HelloThereReply
{
    public HelloThereReply(string message)
    {
        Message = message;
    }
}
