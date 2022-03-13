namespace SimpleSchedulerEmail;

public interface IEmailer
{
    void SendEmailToAdmin(string subject, string bodyHTML);
    Task SendEmailToAdminAsync(string subject, string bodyHTML);
    void SendEmail(string[] toAddresses, string subject, string bodyHTML);
    Task SendEmailAsync(string[] toAddresses, string subject, string bodyHTML);
}
