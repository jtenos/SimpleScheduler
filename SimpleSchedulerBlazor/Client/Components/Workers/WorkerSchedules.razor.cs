using Microsoft.AspNetCore.Components;
using SimpleSchedulerApiModels;

namespace SimpleSchedulerBlazor.Client.Components.Workers;

partial class WorkerSchedules
{
    [Parameter]
    [EditorRequired]
    public WorkerWithSchedules Worker { get; set; } = default!;
}
