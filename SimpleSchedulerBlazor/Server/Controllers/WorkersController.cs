using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OneOf.Types;
using SimpleSchedulerAppServices.Interfaces;
using SimpleSchedulerModels.ApiModels.Workers;
using SimpleSchedulerModels.ResultTypes;

namespace SimpleSchedulerBlazor.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class WorkersController
    : ControllerBase
{
    private readonly IWorkerManager _workerManager;

    public WorkersController(IWorkerManager workerManager)
    {
        _workerManager = workerManager;
    }

    [HttpPost("[action]")]
    public async Task<ActionResult<GetAllWorkersResponse>> GetAllWorkers(
        GetAllWorkersRequest request, CancellationToken cancellationToken)
    {
        return new GetAllWorkersResponse(await _workerManager.GetAllWorkersAsync(cancellationToken));
    }

    [HttpPost("[action]")]
    public async Task<ActionResult<GetWorkerResponse>> GetWorker(
        GetWorkerRequest request, CancellationToken cancellationToken)
    {
        return new GetWorkerResponse(await _workerManager.GetWorkerAsync(request.ID, cancellationToken));
    }

    [HttpPost("[action]")]
    public async Task<ActionResult<DeleteWorkerResponse>> DeleteWorker(
        DeleteWorkerRequest request, CancellationToken cancellationToken)
    {
        await _workerManager.DeactivateWorkerAsync(request.ID, cancellationToken);
        return new DeleteWorkerResponse();
    }

    [HttpPost("[action]")]
    public async Task<ActionResult<ReactivateWorkerResponse>> ReactivateWorker(
        ReactivateWorkerRequest request, CancellationToken cancellationToken)
    {
        await _workerManager.ReactivateWorkerAsync(request.ID, cancellationToken);
        return new ReactivateWorkerResponse();
    }

    [HttpPost("[action]")]
    public async Task<ActionResult<RunNowResponse>> RunNow(
        RunNowRequest request, CancellationToken cancellationToken)
    {
        await _workerManager.RunNowAsync(request.ID, cancellationToken);
        return new RunNowResponse();
    }

    [HttpPost("[action]")]
    public async Task<ActionResult<CreateWorkerResponse>> CreateWorker(
        CreateWorkerRequest request, CancellationToken cancellationToken)
    {
        var result = await _workerManager.AddWorkerAsync(
            workerName: request.WorkerName,
            detailedDescription: request.DetailedDescription,
            emailOnSuccess: request.EmailOnSuccess, 
            parentWorkerID: request.ParentWorkerID,
            timeoutMinutes: request.TimeoutMinutes,
            directoryName: request.DirectoryName,
            executable: request.Executable, 
            argumentValues: request.ArgumentValues, 
            cancellationToken: cancellationToken);

        return result.Match<ActionResult<CreateWorkerResponse>>(
            (Success success) =>
            {
                return new CreateWorkerResponse();
            },
            (InvalidExecutable invalidExecutable) =>
            {
                return BadRequest("Invalid Executable");
            },
            (NameAlreadyExists nameAlreadyExists) =>
            {
                return BadRequest("Name Already Exists");
            },
            (CircularReference circularReference) =>
            {
                return BadRequest("This would result in a circular reference of job parents");
            }
        )!;
    }

    [HttpPost("[action]")]
    public async Task<ActionResult<UpdateWorkerResponse>> UpdateWorker(
        UpdateWorkerRequest request, CancellationToken cancellationToken)
    {
        var result = await _workerManager.UpdateWorkerAsync(
            id: request.WorkerID,
            workerName: request.WorkerName,
            detailedDescription: request.DetailedDescription,
            emailOnSuccess: request.EmailOnSuccess,
            parentWorkerID: request.ParentWorkerID,
            timeoutMinutes: request.TimeoutMinutes,
            directoryName: request.DirectoryName,
            executable: request.Executable,
            argumentValues: request.ArgumentValues,
            cancellationToken: cancellationToken);

        return result.Match<ActionResult<UpdateWorkerResponse>>(
            (Success success) =>
            {
                return new UpdateWorkerResponse();
            },
            (InvalidExecutable invalidExecutable) =>
            {
                return BadRequest("Invalid Executable");
            },
            (NameAlreadyExists nameAlreadyExists) =>
            {
                return BadRequest("Name Already Exists");
            },
            (CircularReference circularReference) =>
            {
                return BadRequest("This would result in a circular reference of job parents");
            }
        )!;
    }
}
