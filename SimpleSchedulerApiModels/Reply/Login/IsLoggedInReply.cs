namespace SimpleSchedulerApiModels.Reply.Login;

public class IsLoggedInReply
{
    public IsLoggedInReply() { }

    public IsLoggedInReply(bool isLoggedIn)
    {
        IsLoggedIn = isLoggedIn;
    }

    public bool IsLoggedIn { get; set; }
}
