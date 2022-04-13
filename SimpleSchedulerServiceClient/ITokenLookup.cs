namespace SimpleSchedulerServiceClient;

public interface ITokenLookup
{
    Task<string?> LookupTokenAsync();
}
