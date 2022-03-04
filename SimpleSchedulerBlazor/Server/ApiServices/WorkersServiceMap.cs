using OneOf.Types;
using SimpleSchedulerApiModels;
using SimpleSchedulerApiModels.Reply.Workers;
using SimpleSchedulerApiModels.Request.Workers;
using SimpleSchedulerAppServices.Interfaces;
using SimpleSchedulerModels.ResultTypes;

namespace SimpleSchedulerBlazor.Server.ApiServices;

public static class WorkersServiceMap
{
    public static void MapWorkersService(this WebApplication app)
    {
        app.MapPost("/Workers/CreateWorker",
            async (
                IWorkerManager workerManager,
                CreateWorkerRequest request
            ) =>
            {
                var result = await workerManager.AddWorkerAsync(
                    workerName: request.WorkerName,
                    detailedDescription: request.DetailedDescription,
                    emailOnSuccess: request.EmailOnSuccess,
                    parentWorkerID: request.ParentWorkerID,
                    timeoutMinutes: request.TimeoutMinutes,
                    directoryName: request.DirectoryName,
                    executable: request.Executable,
                    argumentValues: request.ArgumentValues);

                return result.Match(
                    (Success success) =>
                    {
                        return Results.Ok(new CreateWorkerReply());
                    },
                    (InvalidExecutable invalidExecutable) =>
                    {
                        return Results.BadRequest("Invalid executable");
                    },
                    (NameAlreadyExists nameAlreadyExists) =>
                    {
                        return Results.BadRequest("Name already exists");
                    },
                    (CircularReference circularReference) =>
                    {
                        return Results.BadRequest("This would result in a circular reference of job parents");
                    },
                    (Exception ex) =>
                    {
                        return Results.Problem(ex.Message);
                    }
                );
            });

        app.MapPost("/Workers/DeleteWorker",
            async (
                IWorkerManager workerManager,
                DeleteWorkerRequest request
            ) =>
            {
                await workerManager.DeactivateWorkerAsync(request.ID);
                return new DeleteWorkerReply();
            });

        app.MapPost("/Workers/GetAllWorkers",
            async (
                IWorkerManager workerManager,
                IScheduleManager scheduleManager,
                GetAllWorkersRequest request
            ) =>
            {
                Worker[] workers = (await workerManager.GetAllWorkersAsync())
                    .Select(w => ApiModelBuilders.GetWorker(w))
                    .ToArray();
                Schedule[] allSchedules = (await scheduleManager.GetAllSchedulesAsync())
                    .Select(s => ApiModelBuilders.GetSchedule(s))
                    .ToArray();
                return new GetAllWorkersReply(
                    workers: workers.Select(w =>
                    {
                        Schedule[] schedules = allSchedules
                            .Where(s => s.WorkerID == w.ID)
                            .OrderBy(s => s.TimeOfDayUTC)
                            .ThenBy(s => s.RecurBetweenStartUTC)
                            .ToArray();
                        return new WorkerWithSchedules(w, schedules);
                    }).ToArray()
                );
            });

        app.MapPost("/Workers/GetAllActiveWorkerIDNames",
            async (
                IWorkerManager workerManager,
                GetAllActiveWorkerIDNamesRequest request
            ) =>
            {
                Worker[] workers = (await workerManager.GetAllWorkersAsync())
                    .Select(w => ApiModelBuilders.GetWorker(w))
                    .ToArray();
                return new GetAllActiveWorkerIDNamesReply(
                    workers: workers.Select(w => new WorkerIDName(w.ID, w.WorkerName)).ToArray()
                );
            });

        app.MapPost("/Workers/GetWorker",
            async (
                IWorkerManager workerManager,
                IScheduleManager scheduleManager,
                GetWorkerRequest request
            ) =>
            {
                Worker worker = ApiModelBuilders.GetWorker(await workerManager.GetWorkerAsync(request.ID));
                Schedule[] schedules = (await scheduleManager.GetSchedulesForWorkerAsync(request.ID))
                    .Select(s => ApiModelBuilders.GetSchedule(s))
                    .ToArray();

                return new GetWorkerReply(
                    worker: new(worker, schedules)
                );
            });

        app.MapPost("/Workers/ReactivateWorker",
            async (
                IWorkerManager workerManager,
                ReactivateWorkerRequest request
            ) =>
            {
                await workerManager.ReactivateWorkerAsync(request.ID);
                return new ReactivateWorkerReply();
            });

        app.MapPost("/Workers/RunNow",
            async (
                IWorkerManager workerManager,
                RunNowRequest request
            ) =>
            {
                await workerManager.RunNowAsync(request.ID);
                return new RunNowReply();
            });

        app.MapPost("/Workers/UpdateWorker",
            async (
                IWorkerManager workerManager,
                UpdateWorkerRequest request
            ) =>
            {
                var result = await workerManager.UpdateWorkerAsync(
                    id: request.ID,
                    workerName: request.WorkerName,
                    detailedDescription: request.DetailedDescription,
                    emailOnSuccess: request.EmailOnSuccess,
                    parentWorkerID: request.ParentWorkerID,
                    timeoutMinutes: request.TimeoutMinutes,
                    directoryName: request.DirectoryName,
                    executable: request.Executable,
                    argumentValues: request.ArgumentValues);

                return result.Match(
                    (Success success) =>
                    {
                        return Results.Ok(new UpdateWorkerReply());
                    },
                    (InvalidExecutable invalidExecutable) =>
                    {
                        return Results.BadRequest("Invalid Executable");
                    },
                    (NameAlreadyExists nameAlreadyExists) =>
                    {
                        return Results.BadRequest("Name Already Exists");
                    },
                    (CircularReference circularReference) =>
                    {
                        return Results.BadRequest("This would result in a circular reference of job parents");
                    },
                    (Exception ex) =>
                    {
                        throw ex;
                    }
                );
            });
    }
}
