using SimpleSchedulerBlazor.ProtocolBuffers.Types;
using SimpleSchedulerModels;

namespace SimpleSchedulerBlazor.ProtocolBuffers.Messages;

partial class WorkerMessage
{
    public WorkerMessage(Worker worker)
    {
        ID = worker.ID;
        IsActive = worker.IsActive;
        WorkerName = worker.WorkerName;
        DetailedDescription = worker.DetailedDescription;
        EmailOnSuccess = worker.EmailOnSuccess;
        ParentWorkerID = new NullableInt64(worker.ParentWorkerID);
        TimeoutMinutes = worker.TimeoutMinutes;
        DirectoryName = worker.DirectoryName;
        Executable = worker.Executable;
        ArgumentValues = worker.ArgumentValues;
    }
}
