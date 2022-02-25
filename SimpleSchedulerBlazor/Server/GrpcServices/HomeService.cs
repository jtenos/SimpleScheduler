using SimpleScheduler.Blazor.Shared.ServiceContracts;
using SimpleSchedulerApiModels.Reply.Home;
using SimpleSchedulerApiModels.Request.Home;
using SimpleSchedulerConfiguration.Models;

namespace SimpleSchedulerBlazor.Server.GrpcServices;

public class HomeService
    : IHomeService
{
    private readonly AppSettings _appSettings;

    public HomeService(AppSettings appSettings)
    {
        _appSettings = appSettings;
    }

    Task<GetEnvironmentNameReply> IHomeService.GetEnvironmentNameAsync(GetEnvironmentNameRequest request)
    {
        return Task.FromResult(new GetEnvironmentNameReply(
            environmentName: _appSettings.EnvironmentName
        ));
    }

    Task<GetUtcNowReply> IHomeService.GetUtcNowAsync(GetUtcNowRequest request)
    {
        return Task.FromResult(new GetUtcNowReply(
            formattedDateTime: DateTime.UtcNow.ToString("MMM dd yyyy HH\\:mm\\:ss")
        ));
    }

    Task<HelloThereReply> IHomeService.HelloThereAsync(HelloThereRequest request)
    {
        return Task.FromResult(new HelloThereReply(
            message: "Howdy"
        ));
    }
}
