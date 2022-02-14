using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;

namespace SimpleSchedulerBlazor.Client.Pages;

partial class Login
{
    [Inject]
    private HttpClient Http { get; set; } = default!;

    private LoginModel Model { get; set; } = new();

    private class LoginModel
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        public string? Email { get; set; }
    }

    private async void HandleValidSubmit()
    {
        object postData = new { EmailAddress = Model.Email };
        HttpResponseMessage response = await Http.PostAsJsonAsync("Login/SubmitEmail", postData);
        Console.WriteLine(response);
    }
}

