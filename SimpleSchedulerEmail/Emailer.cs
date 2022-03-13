using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace SimpleSchedulerEmail;

public record class Emailer(
    int Port,
    string EmailFrom,
    string AdminEmail,
    string Host,
    string? UserName,
    string? Password,
    string EnvironmentName)
    : IEmailer
{
    void IEmailer.SendEmailToAdmin(string subject, string bodyHTML)
    {
        string[] toAddresses = { AdminEmail };
        ((IEmailer)this).SendEmail(toAddresses, subject, bodyHTML);
    }

    async Task IEmailer.SendEmailToAdminAsync(string subject, string bodyHTML)
    {
        string[] toAddresses = { AdminEmail };
        await ((IEmailer)this).SendEmailAsync(toAddresses, subject, bodyHTML).ConfigureAwait(false);
    }

    async Task IEmailer.SendEmailAsync(string[] toAddresses, string subject, string bodyHTML)
    {
        MimeMessage msg = new();
        foreach (string addr in toAddresses)
        {
            msg.To.Add(new MailboxAddress(addr, addr));
        }

        if (!msg.To.Any())
        {
            msg.To.Add(new MailboxAddress(AdminEmail, AdminEmail));
        }

        msg.From.Add(new MailboxAddress("Scheduler", EmailFrom));
        msg.Subject = $"Scheduler [{EnvironmentName}]: {subject}";

        BodyBuilder bodyBuilder = new() { HtmlBody = bodyHTML };
        msg.Body = bodyBuilder.ToMessageBody();

        using SmtpClient emailClient = new();

        switch (Port)
        {
            case 587:
                await emailClient.ConnectAsync(Host, Port,
                    SecureSocketOptions.StartTls).ConfigureAwait(false);
                break;
            case 465:
                await emailClient.ConnectAsync(host: Host, port: Port, useSsl: true).ConfigureAwait(false);
                break;
            default:
                await emailClient.ConnectAsync(host: Host,
                    port: Port, options: SecureSocketOptions.None).ConfigureAwait(false);
                break;
        }

        if (!string.IsNullOrWhiteSpace(UserName))
        {
            await emailClient.AuthenticateAsync(UserName, Password).ConfigureAwait(false);
        }
        await emailClient.SendAsync(msg).ConfigureAwait(false);
        await emailClient.DisconnectAsync(quit: true).ConfigureAwait(false);
    }

    void IEmailer.SendEmail(string[] toAddresses, string subject, string bodyHTML)
    {
        MimeMessage msg = new();
        foreach (string addr in toAddresses)
        {
            msg.To.Add(new MailboxAddress(addr, addr));
        }

        if (!msg.To.Any())
        {
            msg.To.Add(new MailboxAddress(AdminEmail, AdminEmail));
        }

        msg.From.Add(new MailboxAddress("Scheduler", EmailFrom));
        msg.Subject = $"Scheduler [{EnvironmentName}]: {subject}";

        BodyBuilder bodyBuilder = new() { HtmlBody = bodyHTML };
        msg.Body = bodyBuilder.ToMessageBody();

        using SmtpClient emailClient = new();

        switch (Port)
        {
            case 587:
                emailClient.Connect(Host, Port,
                    SecureSocketOptions.StartTls);
                break;
            case 465:
                emailClient.Connect(host: Host, port: Port, useSsl: true);
                break;
            default:
                emailClient.Connect(host: Host,
                    port: Port, options: SecureSocketOptions.None);
                break;
        }

        if (!string.IsNullOrWhiteSpace(UserName))
        {
            emailClient.Authenticate(UserName, Password);
        }
        emailClient.Send(msg);
        emailClient.Disconnect(quit: true);
    }
}
