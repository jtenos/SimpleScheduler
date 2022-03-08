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

    [Parameter]
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
                    IsLoading = false;
                },
                async (Error error) =>
                {
                    await Swal.FireAsync("Error", error.Message, SweetAlertIcon.Error);
                    await WorkerDisplayComponent.RefreshAsync();
                }
            );
        }
    }

    private async Task SaveSchedule()
    {
        IsLoading = true;

        if (Schedule.ID == 0)
        {
            (await ServiceClient.TryPostAsync<CreateScheduleRequest, CreateScheduleReply>(
                "Schedules/CreateSchedule",
                new CreateScheduleRequest(
                    Schedule.WorkerID,
                    Schedule.Sunday,
                    Schedule.Monday,
                    Schedule.Tuesday,
                    Schedule.Wednesday,
                    Schedule.Thursday,
                    Schedule.Friday,
                    Schedule.Saturday,
                    Schedule.TimeOfDayUTC,
                    Schedule.RecurTime,
                    Schedule.RecurBetweenStartUTC,
                    Schedule.RecurBetweenEndUTC
                )
            )).Switch(
                async (CreateScheduleReply reply) =>
                {
                    IsEditing = false;
                    await WorkerDisplayComponent.RefreshAsync();
                },
                async (Error error) =>
                {
                    await Swal.FireAsync("Error", error.Message, SweetAlertIcon.Error);
                }
            );
        }
        else
        {
            (await ServiceClient.TryPostAsync<UpdateScheduleRequest, UpdateScheduleReply>(
                "Schedules/UpdateSchedule",
                new UpdateScheduleRequest(
                    Schedule.ID,
                    Schedule.Sunday,
                    Schedule.Monday,
                    Schedule.Tuesday,
                    Schedule.Wednesday,
                    Schedule.Thursday,
                    Schedule.Friday,
                    Schedule.Saturday,
                    Schedule.TimeOfDayUTC,
                    Schedule.RecurTime,
                    Schedule.RecurBetweenStartUTC,
                    Schedule.RecurBetweenEndUTC
                )
            )).Switch(
                async (UpdateScheduleReply reply) =>
                {
                    IsEditing = false;
                    await WorkerDisplayComponent.RefreshAsync();
                },
                async (Error error) =>
                {
                    await Swal.FireAsync("Error", error.Message, SweetAlertIcon.Error);
                }
            );
        }

        IsLoading = false;
    }

    private async Task CancelEditSchedule()
    {
        IsEditing = false;
        await WorkerDisplayComponent.RefreshAsync();
    }
}
