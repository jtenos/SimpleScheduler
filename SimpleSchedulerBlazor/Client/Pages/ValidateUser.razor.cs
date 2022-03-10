using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using SimpleSchedulerApiModels.Reply.Login;
using SimpleSchedulerApiModels.Request.Login;

namespace SimpleSchedulerBlazor.Client.Pages;

partial class ValidateUser
{
    [Inject]
    private ServiceClient ServiceClient { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private ILocalStorageService LocalStorage { get; set; } = default!;

    [Inject]
    private ClientAppInfo ClientAppInfo { get; set; } = default!;

    [Parameter]
    public string ValidationCode { get; set; } = default!;

    public string ErrorMessage { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        (Error? error, ValidateEmailReply? reply) = await ServiceClient.PostAsync<ValidateEmailRequest, ValidateEmailReply>(
            "Login/ValidateEmail",
            new(Guid.Parse(ValidationCode))
        );

        if (error is not null)
        {
            ErrorMessage = error.Message;
            return;
        }

        await LocalStorage.SetItemAsStringAsync($"Jwt:{ClientAppInfo.EnvironmentName}", reply!.JwtToken);
        NavigationManager.NavigateTo("/");
    }
}
