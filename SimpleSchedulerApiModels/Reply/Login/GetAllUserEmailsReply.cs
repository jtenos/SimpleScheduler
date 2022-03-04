namespace SimpleSchedulerApiModels.Reply.Login;

public class GetAllUserEmailsReply
{
    public GetAllUserEmailsReply() { }

    public GetAllUserEmailsReply(string[] emailAddresses)
    {
        EmailAddresses = emailAddresses;
    }

    public string[] EmailAddresses { get; set; } = default!;
}
