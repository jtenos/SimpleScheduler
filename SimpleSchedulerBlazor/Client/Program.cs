using Blazored.LocalStorage;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ProtoBuf.Grpc.Client;
using SimpleScheduler.Blazor.Shared.ServiceContracts;
using SimpleSchedulerBlazor.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddBlazoredLocalStorage();

//builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

static TServiceProxy GetServiceProxy<TServiceProxy>(IServiceProvider services)
    where TServiceProxy : class
{
    HttpClient httpClient = new(new GrpcWebHandler(GrpcWebMode.GrpcWeb, new HttpClientHandler()));
    string baseUri = services.GetRequiredService<NavigationManager>().BaseUri;
    GrpcChannel channel = GrpcChannel.ForAddress(baseUri, new GrpcChannelOptions { HttpClient = httpClient });
    return channel.CreateGrpcService<TServiceProxy>();
}

builder.Services.AddSingleton<IHomeService>(services => GetServiceProxy<IHomeService>(services));
builder.Services.AddSingleton<IJobsService>(services => GetServiceProxy<IJobsService>(services));
builder.Services.AddSingleton<ILoginService>(services => GetServiceProxy<ILoginService>(services));
builder.Services.AddSingleton<ISchedulesService>(services => GetServiceProxy<ISchedulesService>(services));
builder.Services.AddSingleton<IWorkersService>(services => GetServiceProxy<IWorkersService>(services));

builder.Services.AddSingleton(new ClientAppInfo());

await builder.Build().RunAsync();
