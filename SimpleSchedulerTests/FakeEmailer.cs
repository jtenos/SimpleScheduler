using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SimpleSchedulerEmail;

namespace SimpleSchedulerTests
{
    public class FakeEmailer
        : IEmailer
    {
        public static readonly Dictionary<Guid, (IEnumerable<string> toAddresses, string subject, string bodyHTML)> Messages = new();

        public static Guid CurrentGuid { get; set; }

        public Task SendEmailAsync(IEnumerable<string> toAddresses, string subject, string bodyHTML, CancellationToken cancellationToken)
        {
            Messages[CurrentGuid] = (toAddresses, subject, bodyHTML);
            return Task.CompletedTask;
        }

        public Task SendEmailToAdminAsync(string subject, string bodyHTML, CancellationToken cancellationToken)
            => Task.CompletedTask;
    }
}
