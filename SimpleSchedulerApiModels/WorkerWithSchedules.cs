namespace SimpleSchedulerApiModels;

public record class WorkerWithSchedules(
    Worker Worker,
    Schedule[] Schedules
);
