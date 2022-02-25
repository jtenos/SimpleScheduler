using System.Runtime.Serialization;

namespace SimpleSchedulerApiModels.Reply.Login;

[DataContract]
public class GetAllUserEmailsReply
{
    public GetAllUserEmailsReply() { }

    public GetAllUserEmailsReply(string[] emailAddresses)
    {
        EmailAddresses = emailAddresses;
    }

    [DataMember(Order = 1)] public string[] EmailAddresses { get; set; } = default!;
}
