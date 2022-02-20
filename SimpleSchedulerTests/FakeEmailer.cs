using SimpleSchedulerEmail;
using System.Collections.Immutable;

namespace SimpleSchedulerTests;

public class FakeEmailer
    : IEmailer
{
    private readonly List<(ImmutableArray<string> toAddresses, string subject, string bodyHTML)> _messages = new();

    public List<(ImmutableArray<string> toAddresses, string subject, string bodyHTML)> Messages => _messages;

    Task IEmailer.SendEmailAsync(ImmutableArray<string> toAddresses, string subject, string bodyHTML, CancellationToken cancellationToken)
    {
        _messages.Add((toAddresses, subject, bodyHTML));
        return Task.CompletedTask;
    }

    Task IEmailer.SendEmailToAdminAsync(string subject, string bodyHTML, CancellationToken cancellationToken)
    {
        return ((IEmailer)this).SendEmailAsync(new[] { "admin@example.com" }.ToImmutableArray(), subject, bodyHTML, cancellationToken);
    }
}
