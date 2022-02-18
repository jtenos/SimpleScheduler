using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace SimpleSchedulerEmail;

public class Emailer
    : IEmailer
{
    private readonly IConfiguration _config;

    public Emailer(IConfiguration config) => _config = config;

    async Task IEmailer.SendEmailToAdminAsync(string subject, string bodyHTML, CancellationToken cancellationToken)
        => await ((IEmailer)this).SendEmailAsync(new[] { _config.GetValue<string>("MailSettings:AdminEmail") ?? "" }, subject, bodyHTML,
    cancellationToken).ConfigureAwait(false);

    async Task IEmailer.SendEmailAsync(IEnumerable<string> toAddresses, string subject, string bodyHTML,
        CancellationToken cancellationToken)
    {
        int smtpPort = _config.GetValue<int?>("MailSettings:Port") ?? 25;
        string emailFrom = _config.GetValue<string>("MailSettings:EmailFrom");
        string adminEmail = _config.GetValue<string>("MailSettings:AdminEmail");
        string emailHost = _config.GetValue<string>("MailSettings:Host");
        string smtpUserName = _config.GetValue<string?>("MailSettings:UserName") ?? "";
        string smtpPassword = _config.GetValue<string?>("MailSettings:Password") ?? "";

        var msg = new MimeMessage();
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

        var bodyBuilder = new BodyBuilder { HtmlBody = bodyHTML };
        msg.Body = bodyBuilder.ToMessageBody();

        using var emailClient = new SmtpClient();

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
