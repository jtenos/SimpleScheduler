namespace SimpleSchedulerApiModels;

public class WorkerWithSchedules
{
    public WorkerWithSchedules() { }

    public WorkerWithSchedules(Worker worker, Schedule[] schedules)
    {
        Worker = worker;
        Schedules = schedules;
    }

    public Worker Worker { get; set; } = default!;
    public Schedule[] Schedules { get; set; } = Array.Empty<Schedule>();
}
