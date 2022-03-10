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
        Worker.Schedules = Worker.Schedules.Union(new[] { GetDummySchedule(Worker.Worker.ID) }).ToArray();
        StateHasChanged();
        return Task.CompletedTask;
    }

    private static Schedule GetDummySchedule(long workerID)
    {
        return new Schedule
        {
            WorkerID = workerID,
            Sunday = true,
            Monday = true,
            Tuesday = true,
            Wednesday = true,
            Thursday = true,
            Friday = true,
            Saturday = true,
            RecurTime = TimeSpan.FromHours(1)
        };
    }
}
