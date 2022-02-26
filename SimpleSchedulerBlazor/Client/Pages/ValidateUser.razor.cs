using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using SimpleScheduler.Blazor.Shared.ServiceContracts;
using SimpleSchedulerApiModels.Reply.Login;
using SimpleSchedulerApiModels.Request.Login;

namespace SimpleSchedulerBlazor.Client.Pages;

partial class ValidateUser
{
    [Inject]
    private ILoginService LoginService { get; set; } = default!;

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
        try
        {
            ValidateEmailRequest request = new(Guid.Parse(ValidationCode));
            ValidateEmailReply reply = await LoginService.ValidateEmailAsync(request);
            await LocalStorage.SetItemAsStringAsync($"Jwt:{await ClientAppInfo.GetEnvironmentNameAsync()}", reply.JwtToken);
            NavigationManager.NavigateTo("/");
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }
}
