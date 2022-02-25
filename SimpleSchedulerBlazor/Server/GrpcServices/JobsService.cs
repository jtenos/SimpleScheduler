using Grpc.Core;
using OneOf.Types;
using SimpleScheduler.Blazor.Shared.ServiceContracts;
using SimpleSchedulerApiModels;
using SimpleSchedulerApiModels.Reply.Jobs;
using SimpleSchedulerApiModels.Request.Jobs;
using SimpleSchedulerAppServices.Interfaces;
using SimpleSchedulerModels.ResultTypes;

namespace SimpleSchedulerBlazor.Server.GrpcServices;

public class JobsService
    : IJobsService
{
    private readonly IJobManager _jobManager;

    public JobsService(IJobManager jobManager)
    {
        _jobManager = jobManager;
    }

    async Task<AcknowledgeErrorReply> IJobsService.AcknowledgeErrorAsync(AcknowledgeErrorRequest request)
    {
        await _jobManager.AcknowledgeErrorAsync(request.ID);
        return new();
    }

    async Task<CancelJobReply> IJobsService.CancelJobAsync(CancelJobRequest request)
    {
        var result = await _jobManager.CancelJobAsync(request.ID);

        return result.Match(
            (Success success) =>
            {
                return new CancelJobReply();
            },
            (AlreadyCompleted alreadyCompleted) =>
            {
                throw new RpcException(new Status(StatusCode.Aborted, "Job is already completed, unable to cancel"));
            },
            (AlreadyStarted alreadyStarted) =>
            {
                throw new RpcException(new Status(StatusCode.Aborted, "Job is already started, unable to cancel"));
            }
        );
    }

    async Task<GetDetailedMessageReply> IJobsService.GetDetailedMessageAsync(GetDetailedMessageRequest request)
    {
        return new(
            await _jobManager.GetDetailedMessageAsync(request.ID)
        );
    }

    async Task<GetJobsReply> IJobsService.GetJobsAsync(GetJobsRequest request)
    {
        Job[] jobs = (await _jobManager.GetLatestJobsAsync(
            pageNumber: request.PageNumber,
            rowsPerPage: 100,
            statusCode: request.StatusCode,
            workerID: request.WorkerID,
            overdueOnly: request.OverdueOnly
        )).Select(j => ApiModelBuilders.GetJob(j)).ToArray();

        return new(jobs: jobs);
    }
}
