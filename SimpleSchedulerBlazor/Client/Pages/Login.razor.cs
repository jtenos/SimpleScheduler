using System.ComponentModel.DataAnnotations;
using Grpc.Core;
using Microsoft.AspNetCore.Components;
using SimpleSchedulerBlazor.ProtocolBuffers.Messages.Login;
using static SimpleSchedulerBlazor.ProtocolBuffers.Services.LoginService;

namespace SimpleSchedulerBlazor.Client.Pages;

partial class Login
{
    [Inject]
    private LoginServiceClient LoginService { get; set; } = default!;

    private LoginModel Model { get; set; } = new();

    private string? Message { get; set; }
    private bool Loading { get; set; }

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
        try
        {
            SubmitEmailReply result = await LoginService.SubmitEmailAsync(postData);
            Message = "Please check your email for a login link";
        }
        catch (RpcException ex)
        {
            Message = ex.Status.Detail;
            if (string.IsNullOrWhiteSpace(Message))
            {
                Message = ex.Message;
            }
        }
        catch (Exception ex)
        {
            Message = ex.Message;
        }
        Loading = false;
    }
}
