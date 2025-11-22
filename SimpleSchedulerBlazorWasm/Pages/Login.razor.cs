using System.ComponentModel.DataAnnotations;
using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using SimpleSchedulerApiModels.Reply.Login;
using SimpleSchedulerApiModels.Request.Login;
using SimpleSchedulerServiceClient;

namespace SimpleSchedulerBlazorWasm.Pages;

partial class Login
{
    [Inject]
    private ServiceClient ServiceClient { get; set; } = default!;

    [Inject]
    private SweetAlertService Swal { get; set; } = default!;

    [Parameter]
    public string? Email { get; set; }

    private LoginModel Model { get; set; } = new();

    private bool Loading { get; set; }
    private bool SubmittedSuccessfully { get; set; }

    private string[] AllEmails { get; set; } = Array.Empty<string>();

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

        if (error is not null || reply?.EmailAddresses.Any() != true)
        {
            return;
        }

        AllEmails = reply.EmailAddresses;

        // If email is provided via URL parameter, set it in the model and auto-submit
        if (!string.IsNullOrWhiteSpace(Email))
        {
            Model.Email = Email;
            
            // Validate the email address before submitting
            var validationContext = new ValidationContext(Model);
            var validationResults = new List<ValidationResult>();
            if (Validator.TryValidateObject(Model, validationContext, validationResults, true))
            {
                await HandleValidSubmit();
            }
            else
            {
                await Swal.FireAsync("Error", "Invalid email address provided in URL", SweetAlertIcon.Error);
            }
        }

        await base.OnParametersSetAsync();
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
