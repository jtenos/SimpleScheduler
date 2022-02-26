using Microsoft.AspNetCore.Components;
using SimpleSchedulerApiModels;

namespace SimpleSchedulerBlazor.Client.Components.Workers;

partial class WorkerGroupDisplay
{
    [Parameter]
    [EditorRequired]
    public Worker[] AllWorkers { get; set; } = default!;

    [Parameter]
    [EditorRequired]
    public WorkerWithSchedules?[] Workers { get; set; } = default!;
}
