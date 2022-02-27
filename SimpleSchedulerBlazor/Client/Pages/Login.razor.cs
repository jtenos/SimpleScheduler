using System.ComponentModel.DataAnnotations;
using CurrieTechnologies.Razor.SweetAlert2;
using Grpc.Core;
using Microsoft.AspNetCore.Components;
using SimpleScheduler.Blazor.Shared.ServiceContracts;
using SimpleSchedulerApiModels.Reply.Login;
using SimpleSchedulerApiModels.Request.Login;

namespace SimpleSchedulerBlazor.Client.Pages;

partial class Login
{
    [Inject]
    private ILoginService LoginService { get; set; } = default!;

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
        SubmitEmailRequest postData = new(emailAddress: Model.Email!);
        string message;

        // TODO: Find this pattern of try/catch with RpcException and standardize it
        try
        {
            SubmitEmailReply result = await LoginService.SubmitEmailAsync(postData);
            message = "Please check your email for a login link";
            SubmittedSuccessfully = true;
        }
        catch (RpcException ex)
        {
            message = ex.Status.Detail;
            if (string.IsNullOrWhiteSpace(message))
            {
                message = ex.Message;
            }
        }
        catch (Exception ex)
        {
            message = ex.Message;
        }

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
