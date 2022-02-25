namespace SimpleSchedulerDataEntities;

public record class LoginAttemptEntity(
    long ID,
    DateTime SubmitDateUTC,
    string EmailAddress,
    string ValidationCode,
    DateTime? ValidateDateUTC
);
