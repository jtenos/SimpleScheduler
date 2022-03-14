using Blazored.LocalStorage;
using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SimpleSchedulerBlazor.Client;
using SimpleSchedulerServiceClient;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddBlazoredLocalStorage();

builder.Services.AddSweetAlert2(options => {
    options.Theme = SweetAlertTheme.Bootstrap4;
});

builder.Services.AddScoped(sp => new ServiceClient(
    new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) },
    sp.GetRequiredService<JwtContainer>()
));

builder.Services.AddSingleton<JwtContainer>();
builder.Services.AddSingleton<ClientAppInfo>();

await builder.Build().RunAsync();
