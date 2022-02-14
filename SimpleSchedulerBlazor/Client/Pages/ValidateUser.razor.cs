using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using System.Text.Json;

namespace SimpleSchedulerBlazor.Client.Pages;

partial class ValidateUser
{
    [Inject]
    private HttpClient Http { get; set; } = default!;

    [Inject]
    private ILocalStorageService LocalStorage { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Parameter]
    public string ValidationCode { get; set; } = default!;

    public string ErrorMessage { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        object postData = new { ValidationCode };
        HttpResponseMessage response = await Http.PostAsJsonAsync("Login/ValidateEmail", postData);
        if (response.IsSuccessStatusCode)
        {
            try
            {
                ValidationResponse vr = JsonSerializer.Deserialize<ValidationResponse>(await response.Content.ReadAsStringAsync())!;
                await LocalStorage.SetItemAsStringAsync("JwtToken", vr.jwtToken);
                NavigationManager.NavigateTo("/jobs");
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
