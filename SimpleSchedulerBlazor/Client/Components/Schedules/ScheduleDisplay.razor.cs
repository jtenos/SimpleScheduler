using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using SimpleSchedulerApiModels;
using SimpleSchedulerApiModels.Reply.Schedules;
using SimpleSchedulerApiModels.Request.Schedules;
using SimpleSchedulerBlazor.Client.Components.Workers;
using SimpleSchedulerBlazor.Client.Errors;

namespace SimpleSchedulerBlazor.Client.Components.Schedules;

partial class ScheduleDisplay
{
    [Parameter]
    [EditorRequired]
    public Schedule Schedule { get; set; } = default!;

    [Parameter]
    [EditorRequired]
    public WorkerDisplay WorkerDisplayComponent { get; set; } = default!;

    [Inject]
    private ServiceClient ServiceClient { get; set; } = default!;

    [Inject]
    private SweetAlertService Swal { get; set; } = default!;

    public bool IsLoading { get; set; }
    public bool IsEditing { get; set; }

    private Task EditSchedule()
    {
        IsEditing = true;
        return Task.CompletedTask;
    }

    private async Task DeleteSchedule()
    {
        SweetAlertResult result = await Swal.FireAsync(new SweetAlertOptions
        {
            Title = "Are you sure?",
            Text = "This schedule will be deactivated",
            Icon = SweetAlertIcon.Warning,
            ShowCancelButton = true,
            ConfirmButtonText = "Delete",
            CancelButtonText = "Cancel"
        });

        if (!string.IsNullOrEmpty(result.Value))
        {
            IsLoading = true;

            (await ServiceClient.TryPostAsync<DeleteScheduleRequest, DeleteScheduleReply>(
                "Schedules/DeleteSchedule",
                new DeleteScheduleRequest(id: Schedule.ID)
            )).Switch(
                async (DeleteScheduleReply reply) =>
                {
                    await WorkerDisplayComponent.RefreshAsync();
                },
                async (Error error) =>
                {
                    await Swal.FireAsync("Error", error.Message, SweetAlertIcon.Error);
                }
            );
        }
    }

    private async Task SaveSchedule()
    {
        await Task.CompletedTask;
    }

    private async Task CancelEditSchedule()
    {
        await Task.CompletedTask;
    }
}
