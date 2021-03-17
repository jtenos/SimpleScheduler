using System.Collections.Immutable;

namespace SimpleSchedulerModels
{
    public record WorkerDetail(Worker Worker, ImmutableArray<Schedule> Schedules);
}
