using OneOf.Types;
using SimpleSchedulerApiModels;
using SimpleSchedulerApiModels.Reply.Schedules;
using SimpleSchedulerApiModels.Request.Schedules;
using SimpleSchedulerAppServices.Interfaces;

namespace SimpleSchedulerBlazor.Server.ApiServices;

public static class SchedulesServiceMap
{
    public static void MapSchedulesService(this WebApplication app)
    {
        app.MapPost("/Schedules/CreateSchedule",
            async (
                IScheduleManager scheduleManager,
                CreateScheduleRequest request
            ) =>
            {
                return (await scheduleManager.AddScheduleAsync(
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
                )).Match(
                    (Success success) =>
                    {
                        return Results.Ok(new CreateScheduleReply());
                    },
                    (Error<string> error) =>
                    {
                        return Results.BadRequest(error.Value);
                    }
                );
            });

        app.MapPost("/Schedules/DeleteSchedule",
            async (
                IScheduleManager scheduleManager,
                DeleteScheduleRequest request
            ) =>
            {
                await scheduleManager.DeactivateScheduleAsync(request.ID);
                return new DeleteScheduleReply();
            });

        app.MapPost("/Schedules/GetAllSchedules",
            async (
                IScheduleManager scheduleManager,
                GetAllSchedulesRequest request
            ) =>
            {
                Schedule[] schedules = (await scheduleManager.GetAllSchedulesAsync())
                    .Select(s => ApiModelBuilders.GetSchedule(s))
                    .ToArray();
                return new GetAllSchedulesReply(schedules);
            });

        app.MapPost("/Schedules/GetSchedule",
            async (
                IScheduleManager scheduleManager,
                GetScheduleRequest request
            ) =>
            {
                Schedule schedule = ApiModelBuilders.GetSchedule(await scheduleManager.GetScheduleAsync(request.ID));
                return new GetScheduleReply(schedule);
            });

        app.MapPost("/Schedules/ReactivateSchedule",
            async (
                IScheduleManager scheduleManager,
                ReactivateScheduleRequest request
            ) =>
            {
                await scheduleManager.ReactivateScheduleAsync(request.ID);
                return new ReactivateScheduleReply();
            });

        app.MapPost("/Schedules/UpdateSchedule",
            async (
                IScheduleManager scheduleManager,
                UpdateScheduleRequest request
            ) =>
            {
                return (await scheduleManager.UpdateScheduleAsync(
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
                )).Match(
                    (Success success) =>
                    {
                        return Results.Ok(new UpdateScheduleReply());
                    },
                    (Error<string> error) =>
                    {
                        return Results.BadRequest(error.Value);
                    }
                );
            });
    }
}
