namespace SimpleSchedulerBlazor.ProtocolBuffers.Messages.Login;

partial class GetAllUserEmailsRequest
{
}

partial class GetAllUserEmailsReply
{
    public GetAllUserEmailsReply(IEnumerable<string> emailAddresses)
    {
        foreach (string emailAddress in emailAddresses)
        {
            EmailAddresses.Add(emailAddress);
        }
    }
}
