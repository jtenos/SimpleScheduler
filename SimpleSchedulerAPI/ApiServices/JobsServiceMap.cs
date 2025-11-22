using Microsoft.AspNetCore.Authorization;
using SimpleSchedulerApiModels;
using SimpleSchedulerApiModels.Reply.Jobs;
using SimpleSchedulerApiModels.Request.Jobs;
using SimpleSchedulerAppServices.Interfaces;

namespace SimpleSchedulerAPI.ApiServices;

public static class JobsServiceMap
{
    private const string CommonStyles = @"
        body {
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
            display: flex;
            justify-content: center;
            align-items: center;
            min-height: 100vh;
            margin: 0;
            background-color: #f5f5f5;
        }
        .container {
            background: white;
            padding: 2rem;
            border-radius: 8px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
            text-align: center;
            max-width: 500px;
        }
        .icon {
            font-size: 3rem;
            margin-bottom: 1rem;
        }
        .success-icon {
            color: #28a745;
        }
        .error-icon {
            color: #dc3545;
        }
        h1 {
            color: #333;
            margin: 0 0 0.5rem 0;
            font-size: 1.5rem;
        }
        p {
            color: #666;
            margin: 0;
        }";

    [AllowAnonymous]
    private static async Task<AcknowledgeErrorReply> AcknowledgeErrorAsync(
        IJobManager jobManager, AcknowledgeErrorRequest request)
    {
        await jobManager.AcknowledgeErrorAsync(request.AcknowledgementCode);
        return new AcknowledgeErrorReply();
    }

    [AllowAnonymous]
    private static async Task<IResult> AcknowledgeErrorGetAsync(
        IJobManager jobManager, ILogger<IJobManager> logger, Guid acknowledgementCode)
    {
        try
        {
            await jobManager.AcknowledgeErrorAsync(acknowledgementCode);
            
            string html = $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
    <title>Error Acknowledged</title>
    <style>{CommonStyles}</style>
</head>
<body>
    <div class=""container"">
        <div class=""icon success-icon"">✓</div>
        <h1>Error Acknowledged</h1>
        <p>The error has been successfully acknowledged.</p>
    </div>
</body>
</html>";
            
            return Results.Content(html, "text/html");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error acknowledging job with acknowledgement code {AcknowledgementCode}", acknowledgementCode);
            
            string errorHtml = $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
    <title>Error</title>
    <style>{CommonStyles}</style>
</head>
<body>
    <div class=""container"">
        <div class=""icon error-icon"">✕</div>
        <h1>Error</h1>
        <p>Unable to acknowledge the error. The link may be invalid or the error may have already been acknowledged.</p>
    </div>
</body>
</html>";
            
            return Results.Content(errorHtml, "text/html");
        }
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
            rowsPerPage: request.RowsPerPage,
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
        app.MapGet("/Jobs/AcknowledgeError/{acknowledgementCode:guid}", AcknowledgeErrorGetAsync);
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
