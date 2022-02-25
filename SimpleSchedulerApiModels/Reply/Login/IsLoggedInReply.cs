using System.Runtime.Serialization;

namespace SimpleSchedulerApiModels.Reply.Login;

[DataContract]
public class IsLoggedInReply
{
    public IsLoggedInReply() { }

    public IsLoggedInReply(bool isLoggedIn)
    {
        IsLoggedIn = isLoggedIn;
    }

    [DataMember] public bool IsLoggedIn { get; set; }
}
