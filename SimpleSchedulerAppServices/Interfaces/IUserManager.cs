namespace SimpleSchedulerAppServices.Interfaces;

public interface IUserManager
{
    Task<bool> LoginSubmitAsync(string emailAddress, string webUrl);
    Task<string[]> GetAllUserEmailsAsync(bool allowLoginDropdown);
    Task<string> LoginValidateAsync(Guid validationCode, Guid internalSecretAuthKey);
}
