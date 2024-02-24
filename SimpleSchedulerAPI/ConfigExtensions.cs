namespace SimpleSchedulerAPI;

public static class ConfigExtensions
{
    public static string WorkerPath(this IConfiguration config) => config["WorkerPath"]!;

    public static (
        int Port,
        string EmailFrom,
        string AdminEmail,
        string Host,
        string? UserName,
        string? Password
        ) MailSettings(this IConfiguration config) => (
            Port: config.GetValue<int>("MailSettings:Port"),
            EmailFrom: config["MailSettings:EmailFrom"]!,
            AdminEmail: config["MailSettings:AdminEmail"]!,
            Host: config["MailSettings:Host"]!,
            UserName: config["MailSettings:UserName"],
            Password: config["MailSettings:Password"]
        );

    public static (
        string Issuer,
        string Audience,
        string Key
        ) Jwt(this IConfiguration config) => (
            Issuer: config["Jwt:Issuer"]!,
            Audience: config["Jwt:Audience"]!,
            Key: config["Jwt:Key"]!
        );

    public static string EnvironmentName(this IConfiguration config) => config["EnvironmentName"]!;
    public static bool AllowLoginDropdown(this IConfiguration config) => config.GetValue<bool>("AllowLoginDropdown");
    public static string WebUrl(this IConfiguration config) => config["WebUrl"]!;
    public static Guid InternalSecretAuthKey(this IConfiguration config) => config.GetValue<Guid>("InternalSecretAuthKey");
}
