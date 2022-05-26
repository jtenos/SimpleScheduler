using Microsoft.AspNetCore.Components;
using SimpleSchedulerApiModels;

namespace SimpleSchedulerBlazorWasm.Components.Workers;

partial class WorkerSchedules
{
    [Parameter]
    [EditorRequired]
    public WorkerWithSchedules Worker { get; set; } = default!;

    [Parameter]
    [EditorRequired]
    public WorkerDisplay WorkerDisplayComponent { get; set; } = default!;

    [Parameter]
    [EditorRequired]
    public Worker[] AllWorkers { get; set; } = default!;

    public Task AddSchedule()
    {
        Worker = Worker with { Schedules = Worker.Schedules.Union(new[] { Schedule.GetDummySchedule(Worker.Worker.ID) }).ToArray() };
        StateHasChanged();
        return Task.CompletedTask;
    }
}
