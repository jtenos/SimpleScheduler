using Microsoft.AspNetCore.Components;
using SimpleSchedulerApiModels;

namespace SimpleSchedulerBlazor.Client.Components.Schedules;

partial class ScheduleEdit
{
    private Guid UniqueId { get; } = Guid.NewGuid();


    private const string TIME = "TIME";
    private const string RECUR = "RECUR";
    private string TimeType { get; set; } = TIME;

    [Parameter]
    [EditorRequired]
    public Schedule Schedule { get; set; } = default!;
}
