using System.Text.Json.Serialization;

namespace SimpleSchedulerModels
{
    public record Worker
    {
        [JsonConstructorAttribute]
        public Worker(int WorkerID, bool IsActive, string WorkerName, string DetailedDescription,
            string EmailOnSuccess, int? ParentWorkerID, int TimeoutMinutes, int OverdueMinutes,
            string DirectoryName, string Executable, string ArgumentValues)
            => (this.WorkerID, this.IsActive, this.WorkerName, this.DetailedDescription, this.EmailOnSuccess,
                this.ParentWorkerID, this.TimeoutMinutes, this.OverdueMinutes, this.DirectoryName,
                this.Executable, this.ArgumentValues) = (WorkerID, IsActive, WorkerName, DetailedDescription,
                EmailOnSuccess, ParentWorkerID, TimeoutMinutes, OverdueMinutes, DirectoryName,
                Executable, ArgumentValues);

        public Worker(long WorkerID, string UpdateDateTime, long IsActive, string WorkerName,
            string DetailedDescription, string EmailOnSuccess, long ParentWorkerID,
            long TimeoutMinutes, long OverdueMinutes, string DirectoryName, string Executable,
            string ArgumentValues)
            : this((int)WorkerID, IsActive == 1, WorkerName, DetailedDescription, EmailOnSuccess,
            ParentWorkerID > 0 ? (int)ParentWorkerID : default(int?),
            (int)TimeoutMinutes, (int)OverdueMinutes, DirectoryName, Executable,
            ArgumentValues)
        { }

        public int WorkerID { get; }
        public bool IsActive { get; }
        public string WorkerName { get; }
        public string DetailedDescription { get; }
        public string EmailOnSuccess { get; }
        public int? ParentWorkerID { get; }
        public int TimeoutMinutes { get; }
        public int OverdueMinutes { get; }
        public string DirectoryName { get; }
        public string Executable { get; }
        public string ArgumentValues { get; }
    }
}
