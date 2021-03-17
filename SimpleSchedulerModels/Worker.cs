namespace SimpleSchedulerModels
{
    public record Worker(int WorkerID, bool IsActive, string Description, string FreeText, string EmailOnSuccess,
        int? ParentWorkerID, int TimeoutMinutes, int OverdueMinutes, string DirectoryName, string Executable, string Arguments);
}
