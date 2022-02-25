using System.Runtime.Serialization;

namespace SimpleSchedulerApiModels.Request.Login;

[DataContract]
public class ValidateEmailRequest
{
    public ValidateEmailRequest() { }

    public ValidateEmailRequest(Guid validationCode)
    {
        ValidationCode = validationCode;
    }

    [DataMember(Order = 1)] public Guid ValidationCode { get; set; } = default!;
}

