using System.Collections.Immutable;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using SimpleSchedulerConfiguration.Models;

namespace SimpleSchedulerEmail;

public class Emailer
    : IEmailer
{
    private readonly AppSettings _appSettings;

    public Emailer(AppSettings appSettings)
    {
        _appSettings = appSettings;
    }

    async Task IEmailer.SendEmailToAdminAsync(string subject, string bodyHTML, CancellationToken cancellationToken)
    {
        ImmutableArray<string> toAddresses = new[]
        {
            _appSettings.MailSettings.AdminEmail
        }.ToImmutableArray();
        await ((IEmailer)this).SendEmailAsync(toAddresses, subject, bodyHTML, cancellationToken).ConfigureAwait(false);
    }

    async Task IEmailer.SendEmailAsync(ImmutableArray<string> toAddresses, string subject, string bodyHTML,
        CancellationToken cancellationToken)
    {
        int smtpPort = _appSettings.MailSettings.Port;
        string emailFrom = _appSettings.MailSettings.EmailFrom;
        string adminEmail = _appSettings.MailSettings.AdminEmail;
        string emailHost = _appSettings.MailSettings.Host;
        string? smtpUserName = _appSettings.MailSettings.UserName;
        string? smtpPassword = _appSettings.MailSettings.Password;

        MimeMessage msg = new();
        foreach (string addr in toAddresses)
        {
            msg.To.Add(new MailboxAddress(addr, addr));
        }

        if (!msg.To.Any())
        {
            msg.To.Add(new MailboxAddress(adminEmail, adminEmail));
        }

        msg.From.Add(new MailboxAddress("Scheduler", emailFrom));
        msg.Subject = subject;

        BodyBuilder bodyBuilder = new() { HtmlBody = bodyHTML };
        msg.Body = bodyBuilder.ToMessageBody();

        using SmtpClient emailClient = new();

        switch (smtpPort)
        {
            case 587:
                await emailClient.ConnectAsync(emailHost, smtpPort,
                    SecureSocketOptions.StartTls, cancellationToken).ConfigureAwait(false);
                break;
            case 465:
                await emailClient.ConnectAsync(host: emailHost, port: smtpPort, useSsl: true,
                    cancellationToken: cancellationToken).ConfigureAwait(false);
                break;
            default:
                await emailClient.ConnectAsync(host: emailHost,
                    port: smtpPort, options: SecureSocketOptions.None,
                    cancellationToken: cancellationToken).ConfigureAwait(false);
                break;
        }

        if (!string.IsNullOrWhiteSpace(smtpUserName))
        {
            await emailClient.AuthenticateAsync(smtpUserName, smtpPassword, cancellationToken).ConfigureAwait(false);
        }
        await emailClient.SendAsync(msg, cancellationToken: cancellationToken).ConfigureAwait(false);
        await emailClient.DisconnectAsync(quit: true, cancellationToken).ConfigureAwait(false);
    }
}
