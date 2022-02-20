using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OneOf.Types;
using SimpleSchedulerAppServices.Interfaces;
using SimpleSchedulerModels.ApiModels.Jobs;
using SimpleSchedulerModels.ResultTypes;

namespace SimpleSchedulerBlazor.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class JobsController
    : ControllerBase
{
    private readonly IJobManager _jobManager;

    public JobsController(IJobManager jobManager)
    {
        _jobManager = jobManager;
    }

    [HttpPost("[action]")]
    public async Task<ActionResult<GetJobsResponse>> GetJobs(
        GetJobsRequest request, CancellationToken cancellationToken)
    {
        return new GetJobsResponse(await _jobManager.GetLatestJobsAsync(
            pageNumber: request.PageNumber,
            rowsPerPage: 100,
            statusCode: request.StatusCode,
            workerID: request.WorkerID,
            overdueOnly: request.OverdueOnly,
            cancellationToken: cancellationToken
        ));
    }

    [HttpPost("[action]")]
    public async Task<ActionResult<CancelJobResponse>> CancelJob(
        CancelJobRequest request, CancellationToken cancellationToken)
    {
        var result = await _jobManager.CancelJobAsync(request.ID, cancellationToken);

        return result.Match<ActionResult<CancelJobResponse>>(
            (Success success) =>
            {
                return new CancelJobResponse();
            },
            (AlreadyCompleted alreadyCompleted) =>
            {
                return BadRequest("Job is already completed, unable to cancel");
            },
            (AlreadyStarted alreadyStarted) =>
            {
                return BadRequest("Job is already started, unable to cancel");
            }
        );
    }

    [HttpPost("[action]")]
    public async Task<ActionResult<AcknowledgeErrorResponse>> AcknowledgeError(
        AcknowledgeErrorRequest request, CancellationToken cancellationToken)
    {
        await _jobManager.AcknowledgeErrorAsync(request.ID, cancellationToken);
        return new AcknowledgeErrorResponse();
    }

    [HttpPost("[action]")]
    public async Task<ActionResult<GetDetailedMessageResponse>> GetDetailedMessage(
        GetDetailedMessageRequest request, CancellationToken cancellationToken)
    {
        return new GetDetailedMessageResponse(
            await _jobManager.GetDetailedMessageAsync(request.ID, cancellationToken)
        );
    }
}
