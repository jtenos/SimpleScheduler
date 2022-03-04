namespace SimpleSchedulerApiModels.Request.Login;

public class SubmitEmailRequest
{
    public SubmitEmailRequest() { }

    public SubmitEmailRequest(string emailAddress)
    {
        EmailAddress = emailAddress;
    }

    public string EmailAddress { get; set; } = default!;
}
