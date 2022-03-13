using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace SimpleSchedulerEmail;

public class Emailer
    : IEmailer
{
    private readonly MailConfigSection _configSection;
    private readonly string _environmentName;

    public Emailer(MailConfigSection configSection, string environmentName)
    {
        _configSection = configSection;
        _environmentName = environmentName;
    }

    void IEmailer.SendEmailToAdmin(string subject, string bodyHTML)
    {
        string[] toAddresses = { _configSection.AdminEmail };
        ((IEmailer)this).SendEmail(toAddresses, subject, bodyHTML);
    }

    async Task IEmailer.SendEmailToAdminAsync(string subject, string bodyHTML)
    {
        string[] toAddresses = { _configSection.AdminEmail };
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
            msg.To.Add(new MailboxAddress(_configSection.AdminEmail, _configSection.AdminEmail));
        }

        msg.From.Add(new MailboxAddress("Scheduler", _configSection.EmailFrom));
        msg.Subject = $"Scheduler [{_environmentName}]: {subject}";

        BodyBuilder bodyBuilder = new() { HtmlBody = bodyHTML };
        msg.Body = bodyBuilder.ToMessageBody();

        using SmtpClient emailClient = new();

        switch (_configSection.Port)
        {
            case 587:
                await emailClient.ConnectAsync(_configSection.Host, _configSection.Port,
                    SecureSocketOptions.StartTls).ConfigureAwait(false);
                break;
            case 465:
                await emailClient.ConnectAsync(host: _configSection.Host, port: _configSection.Port, useSsl: true).ConfigureAwait(false);
                break;
            default:
                await emailClient.ConnectAsync(host: _configSection.Host,
                    port: _configSection.Port, options: SecureSocketOptions.None).ConfigureAwait(false);
                break;
        }

        if (!string.IsNullOrWhiteSpace(_configSection.UserName))
        {
            await emailClient.AuthenticateAsync(_configSection.UserName, _configSection.Password).ConfigureAwait(false);
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
            msg.To.Add(new MailboxAddress(_configSection.AdminEmail, _configSection.AdminEmail));
        }

        msg.From.Add(new MailboxAddress("Scheduler", _configSection.EmailFrom));
        msg.Subject = $"Scheduler [{_environmentName}]: {subject}";

        BodyBuilder bodyBuilder = new() { HtmlBody = bodyHTML };
        msg.Body = bodyBuilder.ToMessageBody();

        using SmtpClient emailClient = new();

        switch (_configSection.Port)
        {
            case 587:
                emailClient.Connect(_configSection.Host, _configSection.Port,
                    SecureSocketOptions.StartTls);
                break;
            case 465:
                emailClient.Connect(host: _configSection.Host, port: _configSection.Port, useSsl: true);
                break;
            default:
                emailClient.Connect(host: _configSection.Host,
                    port: _configSection.Port, options: SecureSocketOptions.None);
                break;
        }

        if (!string.IsNullOrWhiteSpace(_configSection.UserName))
        {
            emailClient.Authenticate(_configSection.UserName, _configSection.Password);
        }
        emailClient.Send(msg);
        emailClient.Disconnect(quit: true);
    }
}
