using Microsoft.AspNetCore.Components;
using SimpleSchedulerApiModels;

namespace SimpleSchedulerBlazor.Client.Components.Workers;

partial class WorkerDisplay
{
    [Inject]
    private NavigationManager Nav { get; set; } = default!;

    [Parameter]
    [EditorRequired]
    public WorkerWithSchedules? Worker { get; set; } = default!;

    [Parameter]
    [EditorRequired]
    public Worker[] AllWorkers { get; set; } = default!;

    private string? ParentWorkerName { get; set; }

    protected override Task OnInitializedAsync()
    {
        ParentWorkerName = AllWorkers.SingleOrDefault(w => w.ID == Worker?.Worker.ParentWorkerID)?.WorkerName;
        return Task.CompletedTask;
    }

    private Task EditWorker()
    {
        Nav.NavigateTo($"workers/{Worker!.Worker.ID}");
        return Task.CompletedTask;
    }

    private Task DeleteWorker()
    {
        return Task.CompletedTask;
    }

    private Task RunWorker()
    {
        return Task.CompletedTask;
    }
}
