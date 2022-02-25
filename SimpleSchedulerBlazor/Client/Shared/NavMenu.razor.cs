using Microsoft.AspNetCore.Components;
using SimpleScheduler.Blazor.Shared.ServiceContracts;
using SimpleSchedulerApiModels.Reply.Home;
using SimpleSchedulerApiModels.Request.Home;

namespace SimpleSchedulerBlazor.Client.Shared;

partial class NavMenu
{
    [Inject]
    private IHomeService HomeService { get; set; } = default!;

    [Inject]
    private ClientAppInfo ClientAppInfo { get; set; } = default!;

    [Inject]
    private Microsoft.JSInterop.IJSRuntime JS { get; set; } = default!;

    private bool CollapseNavMenu { get; set; } = true;

    private string? NavMenuCssClass => CollapseNavMenu ? "collapse" : null;

    private void ToggleNavMenu() => CollapseNavMenu = !CollapseNavMenu;

    protected override async Task OnInitializedAsync()
    {
        Console.WriteLine(JS);
        if (ClientAppInfo.EnvironmentName is null)
        {
            try
            {
                GetEnvironmentNameReply envReply = await HomeService.GetEnvironmentNameAsync(new GetEnvironmentNameRequest());
                ClientAppInfo.EnvironmentName = envReply.EnvironmentName;
            }
            catch
            {
                ClientAppInfo.EnvironmentName = "Unknown";
            }
        }
    }
}
