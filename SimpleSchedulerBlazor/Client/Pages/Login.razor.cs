using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using SimpleSchedulerModels.ApiModels;

namespace SimpleSchedulerBlazor.Client.Pages;

partial class Login
{
    [Inject]
    private HttpClient Http { get; set; } = default!;

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
        try
        {
            HttpResponseMessage response = await Http.PostAsJsonAsync("api/Login/SubmitEmail", postData);
            if (response.IsSuccessStatusCode)
            {
                SubmitEmailResponse submitResponse = (await response.Content.ReadFromJsonAsync<SubmitEmailResponse>())!;
                Message = submitResponse.Message;
            }
            else
            {
                Message = await response.Content.ReadAsStringAsync();
            }
        }
        catch (Exception ex)
        {
            Message = ex.Message;
        }
    }
}
