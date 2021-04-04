using System;

namespace SimpleSchedulerEntities
{
    public record WorkerEntity(
        long WorkerID,
        long IsActive,
        string WorkerName,
        string DetailedDescription,
        string EmailOnSuccess,
        long? ParentWorkerID,
        long TimeoutMinutes,
        string DirectoryName,
        string Executable,
        string ArgumentValues
    );
}
