namespace SimpleSchedulerDomainModels;

public record class LoginAttempt(
    long ID, 
    DateTime SubmitDateUTC, 
    string EmailAddress,
    Guid ValidationCode, 
    DateTime? ValidateDateUTC
);
