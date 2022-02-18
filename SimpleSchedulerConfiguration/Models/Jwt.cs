namespace SimpleSchedulerConfiguration.Models;

public record class Jwt
{
    public string Key { get; init; } = default!;
    public string Issuer { get; init; } = default!;
    public string Audience { get; init; } = default!;
}
