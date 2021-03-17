using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleSchedulerEmail
{
    public interface IEmailer
    {
        Task SendEmailToAdminAsync(string subject, string bodyHTML, CancellationToken cancellationToken);
        Task SendEmailAsync(IEnumerable<string> toAddresses, string subject, string bodyHTML,
            CancellationToken cancellationToken);
    }
}
