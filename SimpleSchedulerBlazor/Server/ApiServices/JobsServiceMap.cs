using SimpleSchedulerApiModels;
using SimpleSchedulerApiModels.Reply.Jobs;
using SimpleSchedulerApiModels.Request.Jobs;
using SimpleSchedulerAppServices.Interfaces;

namespace SimpleSchedulerBlazor.Server.ApiServices;

public static class JobsServiceMap
{
    private static async Task<AcknowledgeErrorReply> AcknowledgeErrorAsync(
        IJobManager jobManager, AcknowledgeErrorRequest request)
    {
        await jobManager.AcknowledgeErrorAsync(request.ID);
        return new AcknowledgeErrorReply();
    }

    private static async Task<CancelJobReply> CancelJobAsync(
        IJobManager jobManager, CancelJobRequest request)
    {
        await jobManager.CancelJobAsync(request.ID);
        return new();
    }

    private static async Task<GetDetailedMessageReply> GetDetailedMessageAsync(
        IJobManager jobManager, GetDetailedMessageRequest request)
    {
        return new GetDetailedMessageReply(
            await jobManager.GetDetailedMessageAsync(request.ID)
        );
    }

    private static async Task<GetJobsReply> GetJobsAsync(
        IJobManager jobManager, GetJobsRequest request)
    {
        Job[] jobs = (await jobManager.GetLatestJobsAsync(
            pageNumber: request.PageNumber,
            rowsPerPage: 100,
            statusCode: request.StatusCode,
            workerID: request.WorkerID,
            overdueOnly: request.OverdueOnly
        )).Select(j => ApiModelBuilders.GetJob(j)).ToArray();

        return new GetJobsReply(jobs: jobs);
    }

    public static void MapJobsService(this WebApplication app)
    {
        app.MapPost("/Jobs/AcknowledgeError", AcknowledgeErrorAsync);
        app.MapPost("/Jobs/CancelJob", CancelJobAsync);
        app.MapPost("/Jobs/GetDetailedMessage", GetDetailedMessageAsync);
        app.MapPost("/Jobs/GetJobs", GetJobsAsync);
    }
}
