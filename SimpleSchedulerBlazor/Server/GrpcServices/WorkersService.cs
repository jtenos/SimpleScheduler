using Grpc.Core;
using OneOf.Types;
using SimpleSchedulerAppServices.Interfaces;
using SimpleSchedulerBlazor.ProtocolBuffers.Messages.Workers;
using SimpleSchedulerModels.ResultTypes;
using static SimpleSchedulerBlazor.ProtocolBuffers.Services.WorkersService;

namespace SimpleSchedulerBlazor.Server.GrpcServices;

public class WorkersService
    : WorkersServiceBase
{
    private readonly IWorkerManager _workerManager;

    public WorkersService(IWorkerManager workerManager)
    {
        _workerManager = workerManager;
    }

    public override async Task<CreateWorkerReply> CreateWorker(CreateWorkerRequest request, ServerCallContext context)
    {
        var result = await _workerManager.AddWorkerAsync(
            workerName: request.WorkerName,
            detailedDescription: request.DetailedDescription,
            emailOnSuccess: request.EmailOnSuccess,
            parentWorkerID: request.ParentWorkerID.GetValueOrNull(),
            timeoutMinutes: request.TimeoutMinutes,
            directoryName: request.DirectoryName,
            executable: request.Executable,
            argumentValues: request.ArgumentValues,
            cancellationToken: context.CancellationToken);

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

    public override async Task<DeleteWorkerReply> DeleteWorker(DeleteWorkerRequest request, ServerCallContext context)
    {
        await _workerManager.DeactivateWorkerAsync(request.ID, context.CancellationToken);
        return new DeleteWorkerReply();
    }

    public override async Task<GetAllWorkersReply> GetAllWorkers(GetAllWorkersRequest request, ServerCallContext context)
    {
        return new GetAllWorkersReply(
            workers: await _workerManager.GetAllWorkersAsync(context.CancellationToken)
        );
    }

    public override async Task<GetWorkerReply> GetWorker(GetWorkerRequest request, ServerCallContext context)
    {
        return new GetWorkerReply(
            worker: await _workerManager.GetWorkerAsync(request.ID, context.CancellationToken)
        );
    }

    public override async Task<ReactivateWorkerReply> ReactivateWorker(ReactivateWorkerRequest request, ServerCallContext context)
    {
        await _workerManager.ReactivateWorkerAsync(request.ID, context.CancellationToken);
        return new ReactivateWorkerReply();
    }

    public override async Task<RunNowReply> RunNow(RunNowRequest request, ServerCallContext context)
    {
        await _workerManager.RunNowAsync(request.ID, context.CancellationToken);
        return new RunNowReply();
    }

    public override async Task<UpdateWorkerReply> UpdateWorker(UpdateWorkerRequest request, ServerCallContext context)
    {
        var result = await _workerManager.UpdateWorkerAsync(
            id: request.ID,
            workerName: request.WorkerName,
            detailedDescription: request.DetailedDescription,
            emailOnSuccess: request.EmailOnSuccess,
            parentWorkerID: request.ParentWorkerID.GetValueOrNull(),
            timeoutMinutes: request.TimeoutMinutes,
            directoryName: request.DirectoryName,
            executable: request.Executable,
            argumentValues: request.ArgumentValues,
            cancellationToken: context.CancellationToken);

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
