using System.Threading;
using System.Threading.Tasks;

namespace SimpleSchedulerBusiness
{
    public interface IUserManager
    {
        Task<bool> LoginSubmitAsync(string emailAddress,
            CancellationToken cancellationToken);
        Task<string> LoginValidateAsync(string validationKey, CancellationToken cancellationToken);
    }
}
