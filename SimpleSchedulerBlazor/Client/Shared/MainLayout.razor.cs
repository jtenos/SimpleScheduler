using Microsoft.AspNetCore.Components;

namespace SimpleSchedulerBlazor.Client.Shared;

partial class MainLayout
{
    [Inject]
    private ClientAppInfo ClientAppInfo { get; set; } = default!;

    private string EnvironmentName { get; set; } = default!;

    protected override Task OnInitializedAsync()
    {
        EnvironmentName = ClientAppInfo.EnvironmentName;
        return Task.CompletedTask;
    }
}
