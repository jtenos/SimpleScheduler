namespace SimpleSchedulerApiModels.Reply.Workers;

public record class GetAllActiveWorkerIDNamesReply(
    WorkerIDName[] Workers
);
