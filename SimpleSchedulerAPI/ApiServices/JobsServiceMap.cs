using Microsoft.AspNetCore.Authorization;
using SimpleSchedulerApiModels;
using SimpleSchedulerApiModels.Reply.Jobs;
using SimpleSchedulerApiModels.Request.Jobs;
using SimpleSchedulerAppServices.Interfaces;

namespace SimpleSchedulerAPI.ApiServices;

public static class JobsServiceMap
{
    [AllowAnonymous]
    private static async Task<AcknowledgeErrorReply> AcknowledgeErrorAsync(
        IJobManager jobManager, AcknowledgeErrorRequest request)
    {
        await jobManager.AcknowledgeErrorAsync(request.AcknowledgementCode);
        return new AcknowledgeErrorReply();
    }

    [Authorize]
    private static async Task<CancelJobReply> CancelJobAsync(
        IJobManager jobManager, CancelJobRequest request)
    {
        await jobManager.CancelJobAsync(request.ID);
        return new();
    }

    [Authorize]
    private static async Task<CompleteJobReply> CompleteJobAsync(
        IJobManager jobManager, IConfiguration config, CompleteJobRequest request)
    {
        await jobManager.CompleteJobAsync(
            id: request.ID,
            success: request.Success,
            detailedMessage: request.DetailedMessage,
            adminEmail: config.MailSettings().AdminEmail,
            appUrl: config.WebUrl(),
            environmentName: config.EnvironmentName(),
            workerPath: config.WorkerPath()
        );
        return new();
    }

    [Authorize]
    private static async Task<DequeueScheduledJobsReply> DequeueScheduledJobsAsync(
        IJobManager jobManager, DequeueScheduledJobsRequest request)
    {
        JobWithWorker[] jobs = (await jobManager.DequeueScheduledJobsAsync())
            .Select(ApiModelBuilders.GetJobWithWorker)
            .ToArray();

        return new(jobs);
    }

    [Authorize]
    private static async Task<GetDetailedMessageReply> GetDetailedMessageAsync(
        IJobManager jobManager, IConfiguration config, GetDetailedMessageRequest request)
    {
        return new GetDetailedMessageReply(
            await jobManager.GetDetailedMessageAsync(request.ID, config.WorkerPath())
        );
    }

    [Authorize]
    private static async Task<GetJobReply> GetJobAsync(
        IJobManager jobManager, GetJobRequest request)
    {
        return new(
            Job: ApiModelBuilders.GetJob(await jobManager.GetJobAsync(request.ID))
        );
    }

    [Authorize]
    private static async Task<GetJobsReply> GetJobsAsync(
        IJobManager jobManager, GetJobsRequest request)
    {
        JobWithWorkerID[] jobs = (await jobManager.GetLatestJobsAsync(
            pageNumber: request.PageNumber,
            rowsPerPage: 100,
            statusCode: request.StatusCode,
            workerID: request.WorkerID,
            workerName: request.WorkerName,
            overdueOnly: request.OverdueOnly
        )).Select(ApiModelBuilders.GetJobWithWorkerID).ToArray();

        return new GetJobsReply(Jobs: jobs);
    }

    [Authorize]
    private static async Task<GetOverdueJobsReply> GetOverdueJobsAsync(
        IJobManager jobManager, GetOverdueJobsRequest request)
    {
        return new(
            Jobs: (await jobManager.GetOverdueJobsAsync()).Select(ApiModelBuilders.GetJob).ToArray()
        );
    }

    [Authorize]
    private static async Task<RestartStuckJobsReply> RestartStuckJobsAsync(
        IJobManager jobManager, RestartStuckJobsRequest request)
    {
        await jobManager.RestartStuckJobsAsync();
        return new();
    }

    [Authorize]
    private static async Task<StartDueJobsReply> StartDueJobsAsync(
        IJobManager jobManager, StartDueJobsRequest request)
    {
        int numRunning = await jobManager.StartDueJobsAsync();
        return new(numRunning);
    }

    public static void MapJobsService(this WebApplication app)
    {
        app.MapPost("/Jobs/AcknowledgeError", AcknowledgeErrorAsync);
        app.MapPost("/Jobs/CancelJob", CancelJobAsync);
        app.MapPost("/Jobs/CompleteJob", CompleteJobAsync);
        app.MapPost("/Jobs/DequeueScheduledJobs", DequeueScheduledJobsAsync);
        app.MapPost("/Jobs/GetDetailedMessage", GetDetailedMessageAsync);
        app.MapPost("/Jobs/GetJob", GetJobAsync);
        app.MapPost("/Jobs/GetJobs", GetJobsAsync);
        app.MapPost("/Jobs/GetOverdueJobs", GetOverdueJobsAsync);
        app.MapPost("/Jobs/RestartStuckJobs", RestartStuckJobsAsync);
        app.MapPost("/Jobs/StartDueJobs", StartDueJobsAsync);
    }
}
