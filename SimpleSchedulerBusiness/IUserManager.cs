using System;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleSchedulerBusiness
{
    public interface IUserManager
    {
        Task<int> CountUsersAsync(CancellationToken cancellationToken);
        Task<bool> LoginSubmitAsync(string emailAddress,
            CancellationToken cancellationToken);
        Task<string> LoginValidateAsync(Guid validationKey, CancellationToken cancellationToken);
    }
}
