namespace SimpleSchedulerEmail;

public interface IEmailer
{
    Task SendEmailToAdminAsync(string subject, string bodyHTML);
    Task SendEmailAsync(string[] toAddresses, string subject, string bodyHTML);
}
