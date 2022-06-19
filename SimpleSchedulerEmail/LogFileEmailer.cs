using System.Text.Json;

namespace SimpleSchedulerEmail
{
    public class LogFileEmailer
        : IEmailer
    {
        private readonly string _emailFolder;

        public LogFileEmailer(string emailFolder)
        {
            _emailFolder = emailFolder;
        }

        void IEmailer.SendEmail(string[] toAddresses, string subject, string bodyHTML)
        {
            string fullFileName = Path.Combine(_emailFolder, $"Message_{DateTime.Now:yyyyMMdd\\-HHmmss\\.fff}_{Guid.NewGuid().ToString()[..8]}.txt");
            File.WriteAllText(fullFileName, JsonSerializer.Serialize(new
            {
                ToAddresses = toAddresses,
                Subject = subject,
                BodyHTML = bodyHTML
            }));
        }

        Task IEmailer.SendEmailAsync(string[] toAddresses, string subject, string bodyHTML)
        {
            ((IEmailer)this).SendEmail(toAddresses, subject, bodyHTML);
            return Task.CompletedTask;
        }

        void IEmailer.SendEmailToAdmin(string subject, string bodyHTML)
        {
            string[] toAddresses = { "(ADMIN)" };
            ((IEmailer)this).SendEmail(toAddresses, subject, bodyHTML);
        }

        Task IEmailer.SendEmailToAdminAsync(string subject, string bodyHTML)
        {
            string[] toAddresses = { "(ADMIN)" };
            ((IEmailer)this).SendEmail(toAddresses, subject, bodyHTML);
            return Task.CompletedTask;
        }
    }
}
