using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace SimpleSchedulerEmail
{
    public class Emailer
        : IEmailer
    {
        async Task IEmailer.SendEmailToAdminAsync(string subject, string bodyHTML, CancellationToken cancellationToken)
            => await ((IEmailer)this).SendEmailAsync(new[] { Environment.GetEnvironmentVariable("SCHEDULER_ADMIN_EMAIL") ?? "" }, subject, bodyHTML,
                cancellationToken).ConfigureAwait(false);

        async Task IEmailer.SendEmailAsync(IEnumerable<string> toAddresses, string subject, string bodyHTML,
            CancellationToken cancellationToken)
        {
            int smtpPort = GetSmtpPort();
            bool useSsl = Environment.GetEnvironmentVariable("SCHEDULER_SMTP_SECURE") == "1";
            string emailFrom = Environment.GetEnvironmentVariable("SCHEDULER_EMAIL_FROM") ?? "";
            string adminEmail = Environment.GetEnvironmentVariable("SCHEDULER_ADMIN_EMAIL") ?? "";
            string emailHost = Environment.GetEnvironmentVariable("SCHEDULER_SMTP_HOST") ?? "";
            string smtpUserName = Environment.GetEnvironmentVariable("SCHEDULER_SMTP_USERNAME") ?? "";
            string smtpPassword = Environment.GetEnvironmentVariable("SCHEDULER_SMTP_PASSWORD") ?? "";

            if (string.IsNullOrWhiteSpace(emailFrom))
            {
                throw new ApplicationException("SCHEDULER_EMAIL_FROM environment variable missing");
            }
            if (string.IsNullOrWhiteSpace(emailHost))
            {
                throw new ApplicationException("SCHEDULER_SMTP_HOST environment variable missing");
            }

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

            if (useSsl)
            {
                await emailClient.ConnectAsync(host: emailHost, port: smtpPort, useSsl: true,
                    cancellationToken: cancellationToken).ConfigureAwait(false);
            }
            else
            {
                await emailClient.ConnectAsync(host: emailHost,
                    port: smtpPort, options: SecureSocketOptions.None,
                    cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            if (!string.IsNullOrWhiteSpace(smtpUserName))
            {
                await emailClient.AuthenticateAsync(smtpUserName, smtpPassword, cancellationToken).ConfigureAwait(false);
            }
            await emailClient.SendAsync(msg, cancellationToken: cancellationToken).ConfigureAwait(false);
            await emailClient.DisconnectAsync(quit: true, cancellationToken).ConfigureAwait(false);
        }

        private static int GetSmtpPort()
        {
            string? smtpPortVariable = Environment.GetEnvironmentVariable("SCHEDULER_EMAIL_PORT");
            if (string.IsNullOrWhiteSpace(smtpPortVariable))
            {
                return 0; // This will use the default with MailKit
            }
            if (!int.TryParse(smtpPortVariable, out int smtpPort))
            {
                smtpPort = 0; // If not in the environment variable, this will use the default
            }
            return smtpPort;
        }
    }
}
