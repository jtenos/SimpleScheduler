using System.Text.Json.Serialization;

namespace SimpleSchedulerApiModels;

public class Worker
{
    public Worker() { }

    public Worker(long id, bool isActive, string workerName, string detailedDescription,
        string emailOnSuccess, long? parentWorkerID, int timeoutMinutes,
        string directoryName, string executable, string argumentValues)
    {
        ID = id;
        IsActive = isActive;
        WorkerName = workerName;
        DetailedDescription = detailedDescription;
        EmailOnSuccess = emailOnSuccess;
        ParentWorkerID = parentWorkerID;
        TimeoutMinutes = timeoutMinutes;
        DirectoryName = directoryName;
        Executable = executable;
        ArgumentValues = argumentValues;
    }

    [JsonPropertyName("id")] public long ID { get; set; }
    [JsonPropertyName("active")] public bool IsActive { get; set; }
    [JsonPropertyName("wkrNm")] public string WorkerName { get; set; } = default!;
    [JsonPropertyName("desc")] public string DetailedDescription { get; set; } = default!;
    [JsonPropertyName("emailSuc")] public string EmailOnSuccess { get; set; } = default!;
    [JsonPropertyName("parentWid")] public long? ParentWorkerID { get; set; }
    [JsonPropertyName("tmout")] public int TimeoutMinutes { get; set; }
    [JsonPropertyName("dirNm")] public string DirectoryName { get; set; } = default!;
    [JsonPropertyName("exe")] public string Executable { get; set; } = default!;
    [JsonPropertyName("argVals")] public string ArgumentValues { get; set; } = default!;
}
