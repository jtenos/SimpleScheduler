using Microsoft.AspNetCore.Components;
using SimpleSchedulerApiModels;

namespace SimpleSchedulerBlazor.Client.Components.Schedules;

partial class ScheduleDisplay
{
    [Parameter]
    [EditorRequired]
    public Schedule Schedule { get; set; } = default!;

    public bool IsEditing { get; set; }

    private Task EditSchedule()
    {
        IsEditing = true;
        return Task.CompletedTask;
    }

    private async Task DeleteSchedule()
    {
        await Task.CompletedTask;
    }
}
