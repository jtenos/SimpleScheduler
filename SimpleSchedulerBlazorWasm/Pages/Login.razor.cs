using System.ComponentModel.DataAnnotations;
using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using SimpleSchedulerApiModels.Reply.Login;
using SimpleSchedulerApiModels.Request.Login;
using SimpleSchedulerBlazorWasm.Components;
using SimpleSchedulerServiceClient;

namespace SimpleSchedulerBlazorWasm.Pages;

partial class Login
{
    [Inject]
    private ServiceClient ServiceClient { get; set; } = default!;

    [Inject]
    private SweetAlertService Swal { get; set; } = default!;

    private LoginModel Model { get; set; } = new();

    private bool Loading { get; set; }
    private bool SubmittedSuccessfully { get; set; }
    private bool _autoSubmitted;

    private string[] AllEmails { get; set; } = Array.Empty<string>();

    private IEnumerable<BootstrapDropdownItem<string>> EmailDropdownItems =>
        AllEmails.Select(e => new BootstrapDropdownItem<string>(e, e));

    [Parameter]
    [SupplyParameterFromQuery(Name = "email")]
    public string? Email { get; set; }

    private class LoginModel
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }
    }

    protected override async Task OnParametersSetAsync()
    {
        (Error? error, GetAllUserEmailsReply? reply) = await ServiceClient.PostAsync<GetAllUserEmailsRequest, GetAllUserEmailsReply>(
            "Login/GetAllUserEmails",
            new());

        if (error is null && reply?.EmailAddresses.Any() == true)
        {
            AllEmails = reply.EmailAddresses;
        }

        await base.OnParametersSetAsync();

        if (!_autoSubmitted && !string.IsNullOrWhiteSpace(Email))
        {
            _autoSubmitted = true;
            Model.Email = Email;
            await HandleValidSubmit();
        }
    }

    private async Task HandleValidSubmit()
    {
        Loading = true;
        StateHasChanged();
        (Error? error, SubmitEmailReply? reply) = await ServiceClient.PostAsync<SubmitEmailRequest, SubmitEmailReply>(
            "Login/SubmitEmail",
            new(EmailAddress: Model.Email!)
        );

        Loading = false;
        if (error is not null || reply?.Success != true)
        {
            await Swal.FireAsync("Error", error?.Message ?? "Unknown error", SweetAlertIcon.Error);
        }
        else
        {
            await Swal.FireAsync("Success", "Please check your email for a login link", SweetAlertIcon.Success);
        }
    }
}
