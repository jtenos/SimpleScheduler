namespace SimpleSchedulerServiceChecker;

public static class ConfigExtensions
{
    public static (
        int Port,
        string EmailFrom,
        string AdminEmail,
        string Host,
        string? UserName,
        string? Password
        ) MailSettings(this IConfiguration config) => (
            Port: config.GetValue<int>("MailSettings:Port"),
            EmailFrom: config["MailSettings:EmailFrom"],
            AdminEmail: config["MailSettings:AdminEmail"],
            Host: config["MailSettings:Host"],
            UserName: config["MailSettings:UserName"],
            Password: config["MailSettings:Password"]
        );

    public static string EnvironmentName(this IConfiguration config) => config["EnvironmentName"];
    public static Guid InternalSecretAuthKey(this IConfiguration config) => config.GetValue<Guid>("InternalSecretAuthKey");
}
