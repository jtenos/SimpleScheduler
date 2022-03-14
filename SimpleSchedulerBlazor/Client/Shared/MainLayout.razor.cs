using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using SimpleSchedulerServiceClient;

namespace SimpleSchedulerBlazor.Client.Shared;

partial class MainLayout
{
    [Inject]
    private ClientAppInfo ClientAppInfo { get; set; } = default!;

    [Inject]
    private JwtContainer JwtContainer { get; set; } = default!;

    [Inject]
    private ILocalStorageService LocalStorage { get; set; } = default!;

    private string EnvironmentName { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        EnvironmentName = ClientAppInfo.EnvironmentName;
        JwtContainer.Token = await LocalStorage.GetItemAsStringAsync($"Jwt:{EnvironmentName}");
    }
}
