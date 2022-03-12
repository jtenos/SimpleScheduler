using System.ComponentModel.DataAnnotations;
using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using SimpleSchedulerApiModels.Reply.Login;
using SimpleSchedulerApiModels.Request.Login;

namespace SimpleSchedulerBlazor.Client.Pages;

partial class Login
{
    [Inject]
    private ServiceClient ServiceClient { get; set; } = default!;

    [Inject]
    private SweetAlertService Swal { get; set; } = default!;

    private LoginModel Model { get; set; } = new();

    private bool Loading { get; set; }
    private bool SubmittedSuccessfully { get; set; }

    private class LoginModel
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }
    }

    private async Task HandleValidSubmit()
    {
        Loading = true;
        StateHasChanged();
        (Error? error, _) = await ServiceClient.PostAsync<SubmitEmailRequest, SubmitEmailReply>(
            "Login/SubmitEmail",
            new(EmailAddress: Model.Email!)
        );

        Loading = false;
        if (error is not null)
        {
            await Swal.FireAsync("Error", error.Message, SweetAlertIcon.Error);
        }
        else
        {
            await Swal.FireAsync("Success", "Please check your email for a login link", SweetAlertIcon.Success);
        }
    }
}
