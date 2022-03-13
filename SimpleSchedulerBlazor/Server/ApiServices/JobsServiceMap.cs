using SimpleSchedulerApiModels;
using SimpleSchedulerApiModels.Reply.Jobs;
using SimpleSchedulerApiModels.Request.Jobs;
using SimpleSchedulerAppServices.Interfaces;
using SimpleSchedulerBlazor.Server.Config;

namespace SimpleSchedulerBlazor.Server.ApiServices;

public static class JobsServiceMap
{
    private static async Task<AcknowledgeErrorReply> AcknowledgeErrorAsync(
        IJobManager jobManager, AcknowledgeErrorRequest request)
    {
        await jobManager.AcknowledgeErrorAsync(request.AcknowledgementCode);
        return new AcknowledgeErrorReply();
    }

    private static async Task<CancelJobReply> CancelJobAsync(
        IJobManager jobManager, CancelJobRequest request)
    {
        await jobManager.CancelJobAsync(request.ID);
        return new();
    }

    private static async Task<GetDetailedMessageReply> GetDetailedMessageAsync(
        IJobManager jobManager, AppSettings appSettings, GetDetailedMessageRequest request)
    {
        return new GetDetailedMessageReply(
            await jobManager.GetDetailedMessageAsync(request.ID, appSettings.WorkerPath)
        );
    }

    private static async Task<GetJobReply> GetJobAsync(
        IJobManager jobManager, GetJobRequest request)
    {
        return new(
            Job: ApiModelBuilders.GetJob(await jobManager.GetJobAsync(request.ID))
        );
    }

    private static async Task<GetJobsReply> GetJobsAsync(
        IJobManager jobManager, GetJobsRequest request)
    {
        JobWithWorkerID[] jobs = (await jobManager.GetLatestJobsAsync(
            pageNumber: request.PageNumber,
            rowsPerPage: 100,
            statusCode: request.StatusCode,
            workerID: request.WorkerID,
            overdueOnly: request.OverdueOnly
        )).Select(ApiModelBuilders.GetJobWithWorkerID).ToArray();

        return new GetJobsReply(Jobs: jobs);
    }

    private static async Task<GetOverdueJobsReply> GetOverdueJobsAsync(
        IJobManager jobManager, GetOverdueJobsRequest request)
    {
        return new(
            Jobs: (await jobManager.GetOverdueJobsAsync()).Select(ApiModelBuilders.GetJob).ToArray()
        );
    }

    public static void MapJobsService(this WebApplication app)
    {
        app.MapPost("/Jobs/AcknowledgeError", AcknowledgeErrorAsync);
        app.MapPost("/Jobs/CancelJob", CancelJobAsync);
        app.MapPost("/Jobs/GetDetailedMessage", GetDetailedMessageAsync);
        app.MapPost("/Jobs/GetJob", GetJobAsync);
        app.MapPost("/Jobs/GetJobs", GetJobsAsync);
        app.MapPost("/Jobs/GetOverdueJobs", GetOverdueJobsAsync);
    }
}
