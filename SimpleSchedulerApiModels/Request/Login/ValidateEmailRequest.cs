namespace SimpleSchedulerApiModels.Request.Login;

public record class ValidateEmailRequest(
    Guid ValidationCode
);
