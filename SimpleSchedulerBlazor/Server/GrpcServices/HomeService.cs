using Grpc.Core;
using SimpleSchedulerBlazor.ProtocolBuffers.Messages.Home;
using SimpleSchedulerConfiguration.Models;
using static SimpleSchedulerBlazor.ProtocolBuffers.Services.HomeService;

namespace SimpleSchedulerBlazor.Server.GrpcServices;

public class HomeService
    : HomeServiceBase
{
    private readonly AppSettings _appSettings;

    public HomeService(AppSettings appSettings)
    {
        _appSettings = appSettings;
    }

    public override Task<GetEnvironmentNameReply> GetEnvironmentName(GetEnvironmentNameRequest request, ServerCallContext context)
    {
        return Task.FromResult(new GetEnvironmentNameReply(
            environmentName: _appSettings.EnvironmentName
        ));
    }

    public override Task<GetUtcNowReply> GetUtcNow(GetUtcNowRequest request, ServerCallContext context)
    {
        return Task.FromResult(new GetUtcNowReply(
            formattedDateTime: DateTime.UtcNow.ToString("MMM dd yyyy HH\\:mm\\:ss")
        ));
    }

    public override Task<HelloThereReply> HelloThere(HelloThereRequest request, ServerCallContext context)
    {
        return Task.FromResult(new HelloThereReply(
            message: "Howdy"
        ));
    }
}
