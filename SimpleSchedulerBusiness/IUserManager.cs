using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using OneOf;
using OneOf.Types;
using SimpleSchedulerModels.ResultTypes;

namespace SimpleSchedulerBusiness
{
    public interface IUserManager
    {
        Task<bool> LoginSubmitAsync(string emailAddress,
            CancellationToken cancellationToken);
        Task<ImmutableArray<string>> GetAllUserEmailsAsync(CancellationToken cancellationToken);
        Task<OneOf<string, NotFound, Expired>> LoginValidateAsync(string validationKey, CancellationToken cancellationToken);
    }
}
