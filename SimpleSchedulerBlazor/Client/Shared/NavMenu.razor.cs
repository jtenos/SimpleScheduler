using Microsoft.AspNetCore.Components;
using SimpleSchedulerBlazor.ProtocolBuffers.Messages.Home;
using static SimpleSchedulerBlazor.ProtocolBuffers.Services.HomeService;

namespace SimpleSchedulerBlazor.Client.Shared;

partial class NavMenu
{
    [Inject]
    private HomeServiceClient HomeService { get; set; } = default!;

    private bool CollapseNavMenu { get; set; } = true;

    private string? NavMenuCssClass => CollapseNavMenu ? "collapse" : null;

    public string EnvironmentName { get; set; } = "...";

    private void ToggleNavMenu() => CollapseNavMenu = !CollapseNavMenu;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            GetEnvironmentNameReply envReply = await HomeService.GetEnvironmentNameAsync(new GetEnvironmentNameRequest());
            EnvironmentName = envReply.EnvironmentName;
        }
        catch
        {
            EnvironmentName = "Unknown";
        }
    }
}
