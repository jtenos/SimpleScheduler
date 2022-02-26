using Grpc.Core;
using OneOf.Types;
using SimpleScheduler.Blazor.Shared.ServiceContracts;
using SimpleSchedulerApiModels;
using SimpleSchedulerApiModels.Reply.Workers;
using SimpleSchedulerApiModels.Request.Workers;
using SimpleSchedulerAppServices.Interfaces;
using SimpleSchedulerModels.ResultTypes;

namespace SimpleSchedulerBlazor.Server.GrpcServices;

public class WorkersService
    : IWorkersService
{
    private readonly IWorkerManager _workerManager;
    private readonly IScheduleManager _scheduleManager;

    public WorkersService(IWorkerManager workerManager, IScheduleManager scheduleManager)
    {
        _workerManager = workerManager;
        _scheduleManager = scheduleManager;
    }

    async Task<CreateWorkerReply> IWorkersService.CreateWorkerAsync(CreateWorkerRequest request)
    {
        try
        {
            var result = await _workerManager.AddWorkerAsync(
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
                    return new CreateWorkerReply();
                },
                (InvalidExecutable invalidExecutable) =>
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid executable"));
                },
                (NameAlreadyExists nameAlreadyExists) =>
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Name already exists"));
                },
                (CircularReference circularReference) =>
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "This would result in a circular reference of job parents"));
                },
                (Exception ex) =>
                {
                    throw new RpcException(new Status(StatusCode.Internal, ex.Message));
                }
            );
        }
        catch (Exception ex)
        {
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }

    async Task<DeleteWorkerReply> IWorkersService.DeleteWorkerAsync(DeleteWorkerRequest request)
    {
        await _workerManager.DeactivateWorkerAsync(request.ID);
        return new DeleteWorkerReply();
    }

    async Task<GetAllWorkersReply> IWorkersService.GetAllWorkersAsync(GetAllWorkersRequest request)
    {
        Worker[] workers = (await _workerManager.GetAllWorkersAsync())
            .Select(w => ApiModelBuilders.GetWorker(w))
            .ToArray();
        Schedule[] allSchedules = (await _scheduleManager.GetAllSchedulesAsync())
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
    }

    async Task<GetAllActiveWorkerIDNamesReply> IWorkersService.GetAllActiveWorkerIDNamesAsync(
        GetAllActiveWorkerIDNamesRequest request)
    {
        Worker[] workers = (await _workerManager.GetAllWorkersAsync())
            .Select(w => ApiModelBuilders.GetWorker(w))
            .ToArray();
        return new GetAllActiveWorkerIDNamesReply(
            workers: workers.Select(w => new WorkerIDName(w.ID, w.WorkerName)).ToArray()
        );
    }

    async Task<GetWorkerReply> IWorkersService.GetWorkerAsync(GetWorkerRequest request)
    {
        Worker worker = ApiModelBuilders.GetWorker(await _workerManager.GetWorkerAsync(request.ID));
        return new GetWorkerReply(
            worker: worker
        );
    }

    async Task<ReactivateWorkerReply> IWorkersService.ReactivateWorkerAsync(ReactivateWorkerRequest request)
    {
        await _workerManager.ReactivateWorkerAsync(request.ID);
        return new ReactivateWorkerReply();
    }

    async Task<RunNowReply> IWorkersService.RunNowAsync(RunNowRequest request)
    {
        await _workerManager.RunNowAsync(request.ID);
        return new RunNowReply();
    }

    async Task<UpdateWorkerReply> IWorkersService.UpdateWorkerAsync(UpdateWorkerRequest request)
    {
        var result = await _workerManager.UpdateWorkerAsync(
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
                return new UpdateWorkerReply();
            },
            (InvalidExecutable invalidExecutable) =>
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid Executable"));
            },
            (NameAlreadyExists nameAlreadyExists) =>
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Name Already Exists"));
            },
            (CircularReference circularReference) =>
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "This would result in a circular reference of job parents"));
            },
            (Exception ex) =>
            {
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
        );
    }
}
