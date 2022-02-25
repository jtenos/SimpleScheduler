using System.Runtime.Serialization;

namespace SimpleSchedulerApiModels.Request.Login;

[DataContract]
public class SubmitEmailRequest
{
    public SubmitEmailRequest() { }

    public SubmitEmailRequest(string emailAddress)
    {
        EmailAddress = emailAddress;
    }

    [DataMember(Order = 1)] public string EmailAddress { get; set; } = default!;
}
