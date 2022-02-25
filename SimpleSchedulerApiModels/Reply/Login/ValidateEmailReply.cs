using System.Runtime.Serialization;

namespace SimpleSchedulerApiModels.Reply.Login;

[DataContract]
public class ValidateEmailReply
{
    public ValidateEmailReply() { }

    public ValidateEmailReply(string jwtToken)
    {
        JwtToken = jwtToken;
    }

    [DataMember(Order = 1)] public string JwtToken { get; set; } = default!;
}
