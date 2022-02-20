using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleSchedulerAppServices.Interfaces;
using SimpleSchedulerModels.ApiModels.Schedules;

namespace SimpleSchedulerBlazor.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class SchedulesController 
    : ControllerBase
{
    private readonly IScheduleManager _scheduleManager;

    public SchedulesController(IScheduleManager scheduleManager)
    {
        _scheduleManager = scheduleManager;
    }

    [HttpPost("[action]")]
    public async Task<ActionResult<GetAllSchedulesResponse>> GetAllSchedules(
        GetAllSchedulesRequest request, CancellationToken cancellationToken)
    {
        return new GetAllSchedulesResponse(await _scheduleManager.GetAllSchedulesAsync(cancellationToken));
    }

    [HttpPost("[action]")]
    public async Task<ActionResult<GetScheduleResponse>> GetSchedule(
        GetScheduleRequest request, CancellationToken cancellationToken)
    {
        return new GetScheduleResponse(await _scheduleManager.GetScheduleAsync(request.ID, cancellationToken));
    }

    [HttpPost("[action]")]
    public async Task<ActionResult<DeleteScheduleResponse>> DeleteSchedule(
        DeleteScheduleRequest request, CancellationToken cancellationToken)
    {
        await _scheduleManager.DeactivateScheduleAsync(request.ID, cancellationToken);
        return new DeleteScheduleResponse();
    }

    [HttpPost("[action]")]
    public async Task<ActionResult<ReactivateScheduleResponse>> ReactivateSchedule(
        ReactivateScheduleRequest request, CancellationToken cancellationToken)
    {
        await _scheduleManager.ReactivateScheduleAsync(request.ID, cancellationToken);
        return new ReactivateScheduleResponse();
    }

    [HttpPost("[action]")]
    public async Task<ActionResult<CreateScheduleResponse>> CreateSchedule(
        CreateScheduleRequest request, CancellationToken cancellationToken)
    {
        await _scheduleManager.AddScheduleAsync(
            workerID: request.WorkerID, 
            sunday: request.Sunday,
            monday: request.Monday, 
            tuesday: request.Tuesday, 
            wednesday: request.Wednesday, 
            thursday: request.Thursday, 
            friday: request.Friday, 
            saturday: request.Saturday, 
            timeOfDayUTC: request.TimeOfDayUTC, 
            recurTime: request.RecurTime,
            recurBetweenStartUTC: request.RecurBetweenStartUTC, 
            recurBetweenEndUTC: request.RecurBetweenEndUTC, 
            cancellationToken: cancellationToken);

        return new CreateScheduleResponse();
    }

    [HttpPost("[action]")]
    public async Task<ActionResult<UpdateScheduleResponse>> UpdateSchedule(
        UpdateScheduleRequest request, CancellationToken cancellationToken)
    {
        await _scheduleManager.UpdateScheduleAsync(
            id: request.ID,
            sunday: request.Sunday,
            monday: request.Monday,
            tuesday: request.Tuesday,
            wednesday: request.Wednesday,
            thursday: request.Thursday,
            friday: request.Friday,
            saturday: request.Saturday,
            timeOfDayUTC: request.TimeOfDayUTC,
            recurTime: request.RecurTime,
            recurBetweenStartUTC: request.RecurBetweenStartUTC,
            recurBetweenEndUTC: request.RecurBetweenEndUTC,
            cancellationToken: cancellationToken);

        return new UpdateScheduleResponse();
    }
}
