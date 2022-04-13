using SimpleSchedulerServiceClient;

namespace SimpleSchedulerService;

/// <summary>
/// Instead of looking up with each call, this just stores the value, and can be refreshed
/// as needed inside the application.
/// </summary>
internal class TokenLookup
    : ITokenLookup
{
    public string? Token { get; set; }

    Task<string?> ITokenLookup.LookupTokenAsync() => Task.FromResult(Token);
}
