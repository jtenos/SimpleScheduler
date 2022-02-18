namespace SimpleSchedulerConfiguration.Models;

public record class AppSettings
{
    public MailSettings MailSettings { get; init; } = default!;
    public string WorkerPath { get; init; } = default!;
    public string WebUrl { get; init; } = default!;
    public string EnvironmentName { get; init; } = default!;
    public Jwt Jwt { get; init; } = default!;
    public bool AllowLoginDropDown { get; set; }
}
