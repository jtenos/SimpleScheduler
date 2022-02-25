using SimpleScheduler.Blazor.Shared.ServiceContracts;
using SimpleSchedulerApiModels;
using SimpleSchedulerApiModels.Reply.Schedules;
using SimpleSchedulerApiModels.Request.Schedules;
using SimpleSchedulerAppServices.Interfaces;

namespace SimpleSchedulerBlazor.Server.GrpcServices;

public class SchedulesService
    : ISchedulesService
{
    private readonly IScheduleManager _scheduleManager;

    public SchedulesService(IScheduleManager scheduleManager)
    {
        _scheduleManager = scheduleManager;
    }

    async Task<CreateScheduleReply> ISchedulesService.CreateScheduleAsync(CreateScheduleRequest request)
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
            recurBetweenEndUTC: request.RecurBetweenEndUTC);

        return new();
    }

    async Task<DeleteScheduleReply> ISchedulesService.DeleteScheduleAsync(DeleteScheduleRequest request)
    {
        await _scheduleManager.DeactivateScheduleAsync(request.ID);
        return new();
    }

    async Task<GetAllSchedulesReply> ISchedulesService.GetAllSchedulesAsync(GetAllSchedulesRequest request)
    {
        Schedule[] schedules = (await _scheduleManager.GetAllSchedulesAsync())
            .Select(s => ApiModelBuilders.GetSchedule(s))
            .ToArray();
        return new(schedules);
    }

    async Task<GetScheduleReply> ISchedulesService.GetScheduleAsync(GetScheduleRequest request)
    {
        Schedule schedule = ApiModelBuilders.GetSchedule(await _scheduleManager.GetScheduleAsync(request.ID));
        return new GetScheduleReply(schedule);
    }

    async Task<ReactivateScheduleReply> ISchedulesService.ReactivateScheduleAsync(ReactivateScheduleRequest request)
    {
        await _scheduleManager.ReactivateScheduleAsync(request.ID);
        return new();
    }

    async Task<UpdateScheduleReply> ISchedulesService.UpdateScheduleAsync(UpdateScheduleRequest request)
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
            recurBetweenEndUTC: request.RecurBetweenEndUTC);

        return new();
    }
}
