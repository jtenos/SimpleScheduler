using SimpleSchedulerApiModels.Reply.Home;
using SimpleSchedulerApiModels.Request.Home;
using System.ServiceModel;

namespace SimpleScheduler.Blazor.Shared.ServiceContracts;

[ServiceContract(Name = nameof(IHomeService))]
public interface IHomeService
{
    Task<GetEnvironmentNameReply> GetEnvironmentNameAsync(GetEnvironmentNameRequest request);
    Task<GetUtcNowReply> GetUtcNowAsync(GetUtcNowRequest request);
    Task<HelloThereReply> HelloThereAsync(HelloThereRequest request);
}
