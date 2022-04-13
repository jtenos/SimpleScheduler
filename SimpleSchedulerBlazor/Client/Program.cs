using Blazored.LocalStorage;
using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SimpleSchedulerBlazor.Client;
using SimpleSchedulerServiceClient;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddBlazoredLocalStorage();

builder.Services.AddSweetAlert2(options =>
{
    options.Theme = SweetAlertTheme.Bootstrap4;
});

builder.Services.AddScoped<ITokenLookup, TokenLookup>();

builder.Services.AddScoped(sp =>
{
    ITokenLookup tokenLookup = sp.GetRequiredService<ITokenLookup>();

    Console.WriteLine("Creating new ServiceClient");
    NavigationManager navManager = sp.GetRequiredService<NavigationManager>();
    void redirectToLogin() => navManager.NavigateTo("login");
    ServiceClient sc = new(
        httpClient: new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) },
        tokenLookup: tokenLookup,
        redirectToLogin: redirectToLogin,
        logger: sp.GetRequiredService<ILogger<ServiceClient>>()
    );

    Console.WriteLine("ServiceClient is created");

    return sc;
});

builder.Services.AddSingleton<ClientAppInfo>();

await builder.Build().RunAsync();
