namespace SimpleSchedulerApiModels.Reply.Workers;

<<<<<<< HEAD
public class GetWorkerReply
{
    public GetWorkerReply() { }

    public GetWorkerReply(WorkerWithSchedules worker)
    {
        Worker = worker;
    }

<<<<<<< HEAD
    public Worker Worker { get; set; } = default!;
    [DataMember(Order = 1)] public WorkerWithSchedules Worker { get; set; } = default!;
=======
    public WorkerWithSchedules Worker { get; set; } = default!;
>>>>>>> b104e27 (Removing proxy and running API calls directly.)
}
=======
public record class GetWorkerReply(
    WorkerWithSchedules Worker
);
>>>>>>> 09769d2 (Using record classes for request/reply classes fixes #97)
