using System.Collections.Immutable;

namespace SimpleSchedulerModels
{
    public record WorkerDetail(Worker Worker, Worker? ParentWorker, ImmutableArray<Schedule> Schedules);
}
