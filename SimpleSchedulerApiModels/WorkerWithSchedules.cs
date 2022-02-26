using System.Runtime.Serialization;

namespace SimpleSchedulerApiModels;

[DataContract]
public class WorkerWithSchedules
{
    public WorkerWithSchedules() { }

    public WorkerWithSchedules(Worker worker, Schedule[] schedules)
    {
        Worker = worker;
        Schedules = schedules;
    }

    [DataMember(Order = 1)] public Worker Worker { get; set; } = default!;
    [DataMember(Order = 2)] public Schedule[] Schedules { get; set; } = Array.Empty<Schedule>();
}
