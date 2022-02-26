using SimpleScheduler.Blazor.Shared.ServiceContracts;

namespace SimpleSchedulerBlazor.Client;

public class ClientAppInfo
{
    private readonly IHomeService _homeService;
    public string EnvironmentName { get; private set; } = default!;
    public readonly Guid BuildGuid = Guid.NewGuid();

    public ClientAppInfo(IHomeService homeService)
    {
        _homeService = homeService;
    }

    public async Task<string> GetEnvironmentNameAsync()
    {
        return EnvironmentName ??= (await _homeService.GetEnvironmentNameAsync(new())).EnvironmentName;
    }
}
