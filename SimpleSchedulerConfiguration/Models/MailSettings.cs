namespace SimpleSchedulerConfiguration.Models;

public record class MailSettings
{
    public int Port { get; init; } = 25;
    public string EmailFrom { get; init; } = default!;
    public string AdminEmail { get; init; } = default!;
    public string Host { get; init; } = default!;
    public string? UserName { get; init; }
    public string? Password { get; init; }
}
