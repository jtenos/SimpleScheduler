using System;
using System.Net.Http.Json;
using Blazored.LocalStorage;

namespace SimpleSchedulerBlazor.Client;

public class LoggedInValidator
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;

    const string LS_KEY = "auth";

    public LoggedInValidator(HttpClient httpClient, ILocalStorageService localStorage)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
    }

    public async Task<bool> IsLoggedInAsync()
    { 
        string? authValue = await _localStorage.GetItemAsync<string?>(LS_KEY);
        if (authValue is null) { return false; }

        try
        {
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync<string>("LogIn/ValidateAuthValue", authValue);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
