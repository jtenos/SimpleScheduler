namespace SimpleSchedulerBlazor.ProtocolBuffers.Messages.Login;

partial class ValidateEmailRequest
{
    public ValidateEmailRequest(Guid validationCode)
    {
        ValidationCode = validationCode.ToString("N");
    }
}

partial class ValidateEmailReply
{
    public ValidateEmailReply(string jwtToken)
    {
        JwtToken = jwtToken;
    }
}
