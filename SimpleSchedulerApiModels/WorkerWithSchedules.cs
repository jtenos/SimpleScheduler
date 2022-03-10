using System.Text.Json.Serialization;

namespace SimpleSchedulerApiModels;

public class WorkerWithSchedules
{
    public WorkerWithSchedules() { }

    public WorkerWithSchedules(Worker worker, Schedule[] schedules)
    {
        Worker = worker;
        Schedules = schedules;
    }

    [JsonPropertyName("wkr")] public Worker Worker { get; set; } = default!;
    [JsonPropertyName("scheds")] public Schedule[] Schedules { get; set; } = Array.Empty<Schedule>();
}
