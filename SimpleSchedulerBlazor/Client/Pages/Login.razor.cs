using System.ComponentModel.DataAnnotations;
using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using SimpleSchedulerApiModels.Reply.Login;
using SimpleSchedulerApiModels.Request.Login;
using SimpleSchedulerBlazor.Client.Errors;

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

        string message = "";
        (await ServiceClient.TryPostAsync<SubmitEmailRequest, SubmitEmailReply>(
            "Login/SubmitEmail",
            new(emailAddress: Model.Email!)
        )).Switch(
            (SubmitEmailReply reply) =>
            {
                message = "Please check your email for a login link";
                SubmittedSuccessfully = true;
            },
            (Error error) =>
            {
                message = error.Message;
            }
        );

        Loading = false;
        if (SubmittedSuccessfully)
        {
            await Swal.FireAsync("Success", message, SweetAlertIcon.Success);
        }
        else
        {
            await Swal.FireAsync("Error", message, SweetAlertIcon.Error);
        }
    }
}
