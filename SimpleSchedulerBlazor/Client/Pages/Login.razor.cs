using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using SimpleSchedulerApiProxy;
using SimpleSchedulerModels.ApiModels.Login;

namespace SimpleSchedulerBlazor.Client.Pages;

partial class Login
{
    [Inject]
    private LoginProxy LoginProxy { get; set; } = default!;

    private LoginModel Model { get; set; } = new();

    private string? Message { get; set; }

    private class LoginModel
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        public string? Email { get; set; }
    }

    private async Task HandleValidSubmit()
    {
        SubmitEmailRequest postData = new(EmailAddress: Model.Email!);

        var result = await LoginProxy.SubmitEmail(postData);
        if (result.Success)
        {
            Message = "Please check your email for a login link";
        }
        else
        {
            Message = result.ErrorMessage;
        }
    }
}
