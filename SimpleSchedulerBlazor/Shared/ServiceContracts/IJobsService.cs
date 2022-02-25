using SimpleSchedulerApiModels.Reply.Jobs;
using SimpleSchedulerApiModels.Request.Jobs;
using System.ServiceModel;

namespace SimpleScheduler.Blazor.Shared.ServiceContracts;

[ServiceContract(Name = nameof(IJobsService))]
public interface IJobsService
{
    Task<AcknowledgeErrorReply> AcknowledgeErrorAsync(AcknowledgeErrorRequest request);
    Task<CancelJobReply> CancelJobAsync(CancelJobRequest request);
    Task<GetDetailedMessageReply> GetDetailedMessageAsync(GetDetailedMessageRequest request);
    Task<GetJobsReply> GetJobsAsync(GetJobsRequest request);
}
