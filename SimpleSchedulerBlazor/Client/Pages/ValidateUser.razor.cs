using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using System.Text.Json;

namespace SimpleSchedulerBlazor.Client.Pages;

partial class ValidateUser
{
    [Inject]
    private HttpClient Http { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Parameter]
    public string ValidationCode { get; set; } = default!;

    public string ErrorMessage { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        object postData = new { ValidationCode };
        HttpResponseMessage response = await Http.PostAsJsonAsync("api/Login/ValidateEmail", postData);
        if (response.IsSuccessStatusCode)
        {
            try
            {
                NavigationManager.NavigateTo("/");
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }
        else
        {
            Console.WriteLine(response.Content);
            ErrorMessage = "Error validating login";
        }
    }

    private record class ValidationResponse(string jwtToken);
}
