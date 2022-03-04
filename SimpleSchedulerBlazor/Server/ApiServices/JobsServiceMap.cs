using OneOf.Types;
using SimpleSchedulerApiModels;
using SimpleSchedulerApiModels.Reply.Jobs;
using SimpleSchedulerApiModels.Request.Jobs;
using SimpleSchedulerAppServices.Interfaces;
using SimpleSchedulerModels.ResultTypes;

namespace SimpleSchedulerBlazor.Server.ApiServices;

public static class JobsServiceMap
{
    public static void MapJobsService(this WebApplication app)
    {
        app.MapPost("/Jobs/AcknowledgeError",
            async (
                IJobManager jobManager,
                AcknowledgeErrorRequest request
            ) =>
            {
                await jobManager.AcknowledgeErrorAsync(request.ID);
                return new AcknowledgeErrorReply();
            });

        app.MapPost("/Jobs/CancelJob",
            async (
                IJobManager jobManager,
                CancelJobRequest request
            ) =>
            {
                var result = await jobManager.CancelJobAsync(request.ID);

                return result.Match(
                    (Success success) =>
                    {
                        return Results.Ok(new CancelJobReply());
                    },
                    (AlreadyCompleted alreadyCompleted) =>
                    {
                        return Results.BadRequest("Job is already completed, unable to cancel");
                    },
                    (AlreadyStarted alreadyStarted) =>
                    {
                        return Results.BadRequest("Job is already started, unable to cancel");
                    }
                );
            });

        app.MapPost("/Jobs/GetDetailedMessage",
            async (
                IJobManager jobManager,
                GetDetailedMessageRequest request
            ) =>
            {
                return new GetDetailedMessageReply(
                    await jobManager.GetDetailedMessageAsync(request.ID)
                );
            });

        app.MapPost("/Jobs/GetJobs",
            async (
                IJobManager jobManager,
                GetJobsRequest request
            ) =>
            {
                Job[] jobs = (await jobManager.GetLatestJobsAsync(
                    pageNumber: request.PageNumber,
                    rowsPerPage: 100,
                    statusCode: request.StatusCode,
                    workerID: request.WorkerID,
                    overdueOnly: request.OverdueOnly
                )).Select(j => ApiModelBuilders.GetJob(j)).ToArray();

                return new GetJobsReply(jobs: jobs);
            });
    }
}
