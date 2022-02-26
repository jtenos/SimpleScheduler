using SimpleSchedulerApiModels.Reply.Workers;
using SimpleSchedulerApiModels.Request.Workers;
using System.ServiceModel;

namespace SimpleScheduler.Blazor.Shared.ServiceContracts;

[ServiceContract(Name = nameof(IWorkersService))]
public interface IWorkersService
{
    Task<CreateWorkerReply> CreateWorkerAsync(CreateWorkerRequest request);
    Task<DeleteWorkerReply> DeleteWorkerAsync(DeleteWorkerRequest request);
    Task<GetAllWorkersReply> GetAllWorkersAsync(GetAllWorkersRequest request);
    Task<GetAllActiveWorkerIDNamesReply> GetAllActiveWorkerIDNamesAsync(GetAllActiveWorkerIDNamesRequest request);
    Task<GetWorkerReply> GetWorkerAsync(GetWorkerRequest request);
    Task<ReactivateWorkerReply> ReactivateWorkerAsync(ReactivateWorkerRequest request);
    Task<RunNowReply> RunNowAsync(RunNowRequest request);
    Task<UpdateWorkerReply> UpdateWorkerAsync(UpdateWorkerRequest request);
}
