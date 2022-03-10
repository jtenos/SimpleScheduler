using System.Text.Json.Serialization;

namespace SimpleSchedulerApiModels;

public class WorkerIDName
{
    public WorkerIDName() { }

    public WorkerIDName(long id, string workerName)
    {
        ID = id;
        WorkerName = workerName;
    }

    [JsonPropertyName("id")] public long ID { get; set; }
    [JsonPropertyName("wkrNm")] public string WorkerName { get; set; } = default!;
}
