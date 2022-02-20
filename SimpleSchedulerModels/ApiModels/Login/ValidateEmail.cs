namespace SimpleSchedulerModels.ApiModels.Login;

public record class ValidateEmailRequest(Guid ValidationCode);
public record class ValidateEmailResponse(string JwtToken);
