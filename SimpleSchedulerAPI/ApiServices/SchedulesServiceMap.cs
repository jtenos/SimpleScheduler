using Microsoft.AspNetCore.Authorization;
using SimpleSchedulerApiModels;
using SimpleSchedulerApiModels.Reply.Schedules;
using SimpleSchedulerApiModels.Request.Schedules;
using SimpleSchedulerAppServices.Interfaces;

namespace SimpleSchedulerAPI.ApiServices;

public static class SchedulesServiceMap
{
    [Authorize]
    private static async Task<CreateScheduleReply> CreateScheduleAsync(
        IScheduleManager scheduleManager,
        CreateScheduleRequest request)
    {
        await scheduleManager.AddScheduleAsync(
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
            recurBetweenEndUTC: request.RecurBetweenEndUTC
        );
        return new();
    }

    [Authorize]
    private static async Task<DeleteScheduleReply> DeleteScheduleAsync(
        IScheduleManager scheduleManager,
        DeleteScheduleRequest request)
    {
        await scheduleManager.DeactivateScheduleAsync(request.ID);
        return new DeleteScheduleReply();
    }

    [Authorize]
    private static async Task<GetAllSchedulesReply> GetAllSchedulesAsync(
        IScheduleManager scheduleManager,
        GetAllSchedulesRequest request)
    {
        Schedule[] schedules = (await scheduleManager.GetAllSchedulesAsync())
            .Select(s => ApiModelBuilders.GetSchedule(s))
            .ToArray();
        return new GetAllSchedulesReply(schedules);
    }

    [Authorize]
    private static async Task<GetSchedulesReply> GetSchedulesAsync(
        IScheduleManager scheduleManager,
        GetSchedulesRequest request)
    {
        Schedule[] schedules = (await scheduleManager.GetSchedulesAsync(request.IDs))
         .Select(s => ApiModelBuilders.GetSchedule(s))
         .ToArray();
        return new GetSchedulesReply(schedules);
    }

    [Authorize]
    private static async Task<GetScheduleReply> GetScheduleAsync(
        IScheduleManager scheduleManager,
        GetScheduleRequest request)
    {
        Schedule schedule = ApiModelBuilders.GetSchedule(await scheduleManager.GetScheduleAsync(request.ID));
        return new GetScheduleReply(schedule);
    }

    [Authorize]
    private static async Task<ReactivateScheduleReply> ReactivateScheduleAsync(
        IScheduleManager scheduleManager,
        ReactivateScheduleRequest request)
    {
        await scheduleManager.ReactivateScheduleAsync(request.ID);
        return new ReactivateScheduleReply();
    }

    [Authorize]
    private static async Task<UpdateScheduleReply> UpdateScheduleAsync(
        IScheduleManager scheduleManager,
        UpdateScheduleRequest request)
    {
        await scheduleManager.UpdateScheduleAsync(
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
            recurBetweenEndUTC: request.RecurBetweenEndUTC
        );
        return new UpdateScheduleReply();
    }

    public static void MapSchedulesService(this WebApplication app)
    {
        app.MapPost("/Schedules/CreateSchedule", CreateScheduleAsync);
        app.MapPost("/Schedules/DeleteSchedule", DeleteScheduleAsync);
        app.MapPost("/Schedules/GetAllSchedules", GetAllSchedulesAsync);
        app.MapPost("/Schedules/GetSchedules", GetSchedulesAsync);
        app.MapPost("/Schedules/GetSchedule", GetScheduleAsync);
        app.MapPost("/Schedules/ReactivateSchedule", ReactivateScheduleAsync);
        app.MapPost("/Schedules/UpdateSchedule", UpdateScheduleAsync);
    }
}
