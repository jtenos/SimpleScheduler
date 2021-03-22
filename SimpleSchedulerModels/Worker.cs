namespace SimpleSchedulerModels
{
    public record Worker(int WorkerID, bool IsActive, string WorkerName, string DetailedDescription, string EmailOnSuccess,
        int? ParentWorkerID, int TimeoutMinutes, int OverdueMinutes, string DirectoryName, string Executable, string ArgumentValues);
}
