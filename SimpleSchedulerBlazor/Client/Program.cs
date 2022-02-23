using Blazored.LocalStorage;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SimpleSchedulerBlazor.Client;
using static SimpleSchedulerBlazor.ProtocolBuffers.Home.HomeService;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddBlazoredLocalStorage();

//builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

static GrpcChannel GetChannel(IServiceProvider services)
{
    HttpClient httpClient = new(new GrpcWebHandler(GrpcWebMode.GrpcWeb, new HttpClientHandler()));

    // TODO: use DI to implement Jwt authorization header
    httpClient.DefaultRequestHeaders.Add("Authorization", "abcdef");
    string baseUri = services.GetRequiredService<NavigationManager>().BaseUri;
    return GrpcChannel.ForAddress(baseUri, new GrpcChannelOptions { HttpClient = httpClient });
}

builder.Services.AddSingleton(services => new HomeServiceClient(GetChannel(services)));
builder.Services.AddSingleton(services => new JobsServiceClient(GetChannel(services)));
builder.Services.AddSingleton(services => new LoginServiceClient(GetChannel(services)));
builder.Services.AddSingleton(services => new SchedulesServiceClient(GetChannel(services)));
builder.Services.AddSingleton(services => new WorkersServiceClient(GetChannel(services)));

await builder.Build().RunAsync();
