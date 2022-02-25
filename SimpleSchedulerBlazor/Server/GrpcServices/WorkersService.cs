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

    public WorkersService(IWorkerManager workerManager)
    {
        _workerManager = workerManager;
    }

    async Task<CreateWorkerReply> IWorkersService.CreateWorkerAsync(CreateWorkerRequest request)
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
            }
        );
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
        return new GetAllWorkersReply(
            workers: workers
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
            }
        );
    }
}
