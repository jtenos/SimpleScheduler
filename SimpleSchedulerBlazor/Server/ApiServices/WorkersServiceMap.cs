using SimpleSchedulerApiModels;
using SimpleSchedulerApiModels.Reply.Workers;
using SimpleSchedulerApiModels.Request.Workers;
using SimpleSchedulerAppServices.Interfaces;

namespace SimpleSchedulerBlazor.Server.ApiServices;

public static class WorkersServiceMap
{
    private static async Task<CreateWorkerReply> CreateWorkerAsync(
        IWorkerManager workerManager,
        IConfiguration config,
        CreateWorkerRequest request)
    {
        await workerManager.AddWorkerAsync(
            workerName: request.WorkerName,
            detailedDescription: request.DetailedDescription,
            emailOnSuccess: request.EmailOnSuccess,
            parentWorkerID: request.ParentWorkerID,
            timeoutMinutes: request.TimeoutMinutes,
            directoryName: request.DirectoryName,
            executable: request.Executable,
            argumentValues: request.ArgumentValues,
            workerPath: config.WorkerPath());

        return new();
    }

    private static async Task<DeleteWorkerReply> DeleteWorkerAsync(
        IWorkerManager workerManager,
        DeleteWorkerRequest request)
    {
        await workerManager.DeactivateWorkerAsync(request.ID);
        return new DeleteWorkerReply();
    }

    private static async Task<GetAllWorkersReply> GetAllWorkersAsync(
        IWorkerManager workerManager,
        IScheduleManager scheduleManager,
        GetAllWorkersRequest request)
    {
        Worker[] workers = (await workerManager.GetAllWorkersAsync())
            .Select(w => ApiModelBuilders.GetWorker(w))
            .ToArray();
        Schedule[] allSchedules = (await scheduleManager.GetAllSchedulesAsync())
            .Select(s => ApiModelBuilders.GetSchedule(s))
            .ToArray();
        return new(
            Workers: workers.Select(w =>
            {
                Schedule[] schedules = allSchedules
                    .Where(s => s.WorkerID == w.ID)
                    .OrderBy(s => s.TimeOfDayUTC)
                    .ThenBy(s => s.RecurBetweenStartUTC)
                    .ToArray();
                return new WorkerWithSchedules(w, schedules);
            }).ToArray()
        );
    }

    private static async Task<GetAllActiveWorkerIDNamesReply> GetAllActiveWorkerIDNamesAsync(
        IWorkerManager workerManager,
        GetAllActiveWorkerIDNamesRequest request)
    {
        Worker[] workers = (await workerManager.GetAllWorkersAsync())
                    .Select(w => ApiModelBuilders.GetWorker(w))
                    .ToArray();
        return new(
            Workers: workers.Select(w => new WorkerIDName(w.ID, w.WorkerName)).ToArray()
        );
    }

    private static async Task<GetWorkersReply> GetWorkersAsync(
        IWorkerManager workerManager,
        GetWorkersRequest request)
    {
        Worker[] workers = (await workerManager.GetWorkersAsync(request.IDs))
                    .Select(w => ApiModelBuilders.GetWorker(w))
                    .ToArray();
        return new(workers);
    }

    private static async Task<GetWorkerReply> GetWorkerAsync(
        IWorkerManager workerManager,
        IScheduleManager scheduleManager,
        GetWorkerRequest request)
    {
        Worker worker = ApiModelBuilders.GetWorker(await workerManager.GetWorkerAsync(request.ID));
        Schedule[] schedules = (await scheduleManager.GetSchedulesForWorkerAsync(request.ID))
            .Select(s => ApiModelBuilders.GetSchedule(s))
            .ToArray();

        return new(
            Worker: new(worker, schedules)
        );
    }

    private static async Task<ReactivateWorkerReply> ReactivateWorkerAsync(
        IWorkerManager workerManager,
        ReactivateWorkerRequest request)
    {
        await workerManager.ReactivateWorkerAsync(request.ID);
        return new();
    }

    private static async Task<RunNowReply> RunNowAsync(
        IWorkerManager workerManager,
        RunNowRequest request)
    {
        await workerManager.RunNowAsync(request.ID);
        return new();
    }

    private static async Task<UpdateWorkerReply> UpdateWorkerAsync(
        IWorkerManager workerManager,
        IConfiguration config,
        UpdateWorkerRequest request)
    {
        await workerManager.UpdateWorkerAsync(
            id: request.ID,
            workerName: request.WorkerName,
            detailedDescription: request.DetailedDescription,
            emailOnSuccess: request.EmailOnSuccess,
            parentWorkerID: request.ParentWorkerID,
            timeoutMinutes: request.TimeoutMinutes,
            directoryName: request.DirectoryName,
            executable: request.Executable,
            argumentValues: request.ArgumentValues,
            workerPath: config.WorkerPath());

        return new();
    }

    public static void MapWorkersService(this WebApplication app)
    {
        app.MapPost("/Workers/CreateWorker", CreateWorkerAsync);
        app.MapPost("/Workers/DeleteWorker", DeleteWorkerAsync);
        app.MapPost("/Workers/GetAllWorkers", GetAllWorkersAsync);
        app.MapPost("/Workers/GetAllActiveWorkerIDNames", GetAllActiveWorkerIDNamesAsync);
        app.MapPost("/Workers/GetWorkers", GetWorkersAsync);
        app.MapPost("/Workers/GetWorker", GetWorkerAsync);
        app.MapPost("/Workers/ReactivateWorker", ReactivateWorkerAsync);
        app.MapPost("/Workers/RunNow", RunNowAsync);
        app.MapPost("/Workers/UpdateWorker", UpdateWorkerAsync);
    }
}
