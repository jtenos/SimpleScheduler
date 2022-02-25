using OneOf;
using OneOf.Types;
using SimpleSchedulerModels.ResultTypes;

namespace SimpleSchedulerAppServices.Interfaces;

public interface IUserManager
{
    Task<bool> LoginSubmitAsync(string emailAddress);
    Task<string[]> GetAllUserEmailsAsync();
    Task<OneOf<string, NotFound, Expired>> LoginValidateAsync(Guid validationCode);
}
