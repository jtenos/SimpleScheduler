using Blazored.LocalStorage;
using Blazorise;
using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;
using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SimpleSchedulerBlazorWasm;
using SimpleSchedulerServiceClient;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddBlazoredLocalStorage();

builder.Services.AddSweetAlert2(options => options.Theme = SweetAlertTheme.Bootstrap4);

builder.Services
	.AddBlazorise(options => options.Immediate = true)
	.AddBootstrap5Providers()
	.AddFontAwesomeIcons();

builder.Services.AddScoped<ITokenLookup, TokenLookup>();

builder.Services.AddScoped(sp =>
{
	ITokenLookup tokenLookup = sp.GetRequiredService<ITokenLookup>();

	Console.WriteLine("Creating new ServiceClient");
	NavigationManager navManager = sp.GetRequiredService<NavigationManager>();
	void redirectToLogin() => navManager.NavigateTo("login");
	ServiceClient sc = new(
		httpClient: new HttpClient { BaseAddress = new(builder.Configuration["ApiUrl"]!) },
		tokenLookup: tokenLookup,
		redirectToLogin: redirectToLogin,
		logger: sp.GetRequiredService<ILogger<ServiceClient>>()
	);

	Console.WriteLine("ServiceClient is created");

	return sc;
});

builder.Services.AddSingleton<ClientAppInfo>();

await builder.Build().RunAsync();
