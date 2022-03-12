namespace SimpleSchedulerApiModels.Reply.Workers;

public record class GetAllWorkersReply(
    WorkerWithSchedules[] Workers
);
