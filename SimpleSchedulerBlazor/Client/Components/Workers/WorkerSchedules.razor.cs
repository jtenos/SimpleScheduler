using Microsoft.AspNetCore.Components;
using SimpleSchedulerApiModels;

namespace SimpleSchedulerBlazor.Client.Components.Workers;

partial class WorkerSchedules
{
    [Parameter]
    [EditorRequired]
    public WorkerWithSchedules Worker { get; set; } = default!;

    [Parameter]
    [EditorRequired]
    public WorkerDisplay WorkerDisplayComponent { get; set; } = default!;

    public Task AddSchedule()
    {
        Worker.Schedules = Worker.Schedules.Union(new[] { new Schedule { WorkerID = Worker.Worker.ID } }).ToArray();
        StateHasChanged();
        return Task.CompletedTask;
    }
}
