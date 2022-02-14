using System;
using Microsoft.AspNetCore.Components;

namespace SimpleSchedulerBlazor.Client.Pages;

partial class Index
{
    [Inject]
    private LoggedInValidator LoggedInValidator { get; set; } = default!;
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        bool isLoggedIn = await LoggedInValidator.IsLoggedInAsync();
        if (!isLoggedIn)
        {
            NavigationManager.NavigateTo("login");
            return;
        }
    }
}

