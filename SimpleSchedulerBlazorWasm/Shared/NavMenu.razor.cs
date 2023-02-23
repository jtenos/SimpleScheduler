using Microsoft.AspNetCore.Components;
using SimpleSchedulerApiModels.Reply.Home;
using SimpleSchedulerApiModels.Request.Home;
using SimpleSchedulerServiceClient;
using System.Timers;
using Timer = System.Timers.Timer;

namespace SimpleSchedulerBlazorWasm.Shared;

partial class NavMenu
{
    [Parameter]
    [EditorRequired]
    public string EnvironmentName { get; set; } = default!;

    [Inject]
    private ServiceClient ServiceClient { get; set; } = default!;

    private bool CollapseNavMenu { get; set; } = true;

    private string? NavMenuCssClass => CollapseNavMenu ? "collapse" : null;

    private void ToggleNavMenu() => CollapseNavMenu = !CollapseNavMenu;

    private string UTCNowFormatted { get; set; } = "";

    public NavMenu()
    {
    }

    protected override async Task OnInitializedAsync()
    {
        Timer timer = new(15000);
        timer.Elapsed += async (sender, e) => await RetrieveTimeAsync(sender, e);
        timer.Start();
        await RetrieveTimeAsync(this, default!);
    }

    private async Task RetrieveTimeAsync(object? sender, ElapsedEventArgs e)
    {
        (Error? error, GetUtcNowReply? reply) = await ServiceClient.GetAsync<GetUtcNowReply>(
            "home/getUtcNow",
            forceUnauthenticated: true
        );
        UTCNowFormatted = error?.Message ?? reply!.FormattedDateTime;
        StateHasChanged();
    }
}
