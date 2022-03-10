namespace SimpleSchedulerAppServices.Interfaces;

public interface IUserManager
{
    Task<bool> LoginSubmitAsync(string emailAddress);
    Task<string[]> GetAllUserEmailsAsync();
    Task<string> LoginValidateAsync(Guid validationCode);
}
