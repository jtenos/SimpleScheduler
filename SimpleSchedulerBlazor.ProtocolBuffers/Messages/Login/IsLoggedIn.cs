namespace SimpleSchedulerBlazor.ProtocolBuffers.Messages.Login;

partial class IsLoggedInRequest
{
}

partial class IsLoggedInReply
{
    public IsLoggedInReply(bool isLoggedIn)
    {
        IsLoggedIn = isLoggedIn;
    }
}
