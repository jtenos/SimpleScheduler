namespace SimpleSchedulerApiModels.Reply.Login;

public class ValidateEmailReply
{
    public ValidateEmailReply() { }

    public ValidateEmailReply(string jwtToken)
    {
        JwtToken = jwtToken;
    }

    public string JwtToken { get; set; } = default!;
}
