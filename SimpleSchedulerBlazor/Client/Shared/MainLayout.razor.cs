using Microsoft.AspNetCore.Components;

namespace SimpleSchedulerBlazor.Client.Shared;

partial class MainLayout
{
    [Inject]
    private ClientAppInfo ClientAppInfo { get; set; } = default!;

    private string EnvironmentName { get; set; } = default!;

    protected override async Task OnParametersSetAsync()
    {
        EnvironmentName = ClientAppInfo.EnvironmentName;
        await base.OnParametersSetAsync();
    }
}
