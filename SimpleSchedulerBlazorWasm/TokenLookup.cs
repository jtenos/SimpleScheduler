using Blazored.LocalStorage;
using SimpleSchedulerServiceClient;

namespace SimpleSchedulerBlazorWasm;

public class TokenLookup
    : ITokenLookup
{
    private readonly ILocalStorageService _localStorageService;
    private readonly ClientAppInfo _clientAppInfo;

    public TokenLookup(ILocalStorageService localStorageService, ClientAppInfo clientAppInfo)
    {
        _localStorageService = localStorageService;
        _clientAppInfo = clientAppInfo;
    }

    async Task<string?> ITokenLookup.LookupTokenAsync()
    {
        string environmentName = _clientAppInfo.EnvironmentName;
        return await _localStorageService.GetItemAsStringAsync($"Jwt:{environmentName}");
    }
}
