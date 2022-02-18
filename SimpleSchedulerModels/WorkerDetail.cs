using System.Collections.Immutable;

namespace SimpleSchedulerModels;

public record class WorkerDetail(Worker Worker, Worker? ParentWorker, ImmutableArray<Schedule> Schedules);
