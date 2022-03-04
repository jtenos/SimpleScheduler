namespace SimpleSchedulerApiModels.Request.Login;

public class ValidateEmailRequest
{
    public ValidateEmailRequest() { }

    public ValidateEmailRequest(Guid validationCode)
    {
        ValidationCode = validationCode;
    }

    public Guid ValidationCode { get; set; } = default!;
}

