using Microsoft.AspNetCore.Components;

namespace SimpleSchedulerBlazor.Client.Shared;

partial class NavMenu
{
    [Parameter]
    [EditorRequired]
    public string EnvironmentName { get; set; } = default!;

    private bool CollapseNavMenu { get; set; } = true;

    private string? NavMenuCssClass => CollapseNavMenu ? "collapse" : null;

    private void ToggleNavMenu() => CollapseNavMenu = !CollapseNavMenu;

    public NavMenu()
    {
    }
}
