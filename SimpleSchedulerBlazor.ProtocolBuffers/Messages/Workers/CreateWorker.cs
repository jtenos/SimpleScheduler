using SimpleSchedulerBlazor.ProtocolBuffers.Types;

namespace SimpleSchedulerBlazor.ProtocolBuffers.Messages.Workers;

partial class CreateWorkerRequest
{
    public CreateWorkerRequest(
        string workerName,
        string detailedDescription,
        string emailOnSuccess,
        long? parentWorkerID,
        int timeoutMinutes,
        string directoryName,
        string executable,
        string argumentValues
    )
    {
        WorkerName = workerName;
        DetailedDescription = detailedDescription;
        EmailOnSuccess = emailOnSuccess;
        ParentWorkerID = new NullableInt64(parentWorkerID);
        TimeoutMinutes = timeoutMinutes;
        DirectoryName = directoryName;
        Executable = executable;
        ArgumentValues = argumentValues;
    }
}

partial class CreateWorkerReply
{
}
