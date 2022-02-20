using System.Collections.Immutable;

namespace SimpleSchedulerEmail
{
    public interface IEmailer
    {
        Task SendEmailToAdminAsync(string subject, string bodyHTML, CancellationToken cancellationToken);
        Task SendEmailAsync(ImmutableArray<string> toAddresses, string subject, string bodyHTML,
            CancellationToken cancellationToken);
    }
}
