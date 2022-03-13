﻿using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using SimpleSchedulerApiModels;
using SimpleSchedulerApiModels.Reply.Schedules;
using SimpleSchedulerApiModels.Request.Schedules;
using SimpleSchedulerBlazor.Client.Components.Workers;
using SimpleSchedulerServiceClient;

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
            (Error? error, _) = await ServiceClient.PostAsync<DeleteScheduleRequest, DeleteScheduleReply>(
                "Schedules/DeleteSchedule",
                new DeleteScheduleRequest(ID: Schedule.ID)
            );

            if (error is not null)
            {
                await Swal.FireAsync("Error", error.Message, SweetAlertIcon.Error);
                await WorkerDisplayComponent.RefreshAsync();
                return;
            }

            await WorkerDisplayComponent.RefreshAsync();
            IsLoading = false;
        }
    }

    private async Task SaveSchedule()
    {
        IsLoading = true;

        if (Schedule.ID == 0)
        {
            (Error? error, _) = await ServiceClient.PostAsync<CreateScheduleRequest, CreateScheduleReply>(
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
            );

            if (error is not null)
            {
                await Swal.FireAsync("Error", error.Message, SweetAlertIcon.Error);
                return;
            }

            IsEditing = false;
            await WorkerDisplayComponent.RefreshAsync();
        }
        else
        {
            (Error? error, _) = await ServiceClient.PostAsync<UpdateScheduleRequest, UpdateScheduleReply>(
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
            );

            if (error is not null)
            {
                await Swal.FireAsync("Error", error.Message, SweetAlertIcon.Error);
                IsLoading = false;
                return;
            }

            IsEditing = false;
            await WorkerDisplayComponent.RefreshAsync();
        }

        IsLoading = false;
    }

    private async Task CancelEditSchedule()
    {
        IsEditing = false;
        await WorkerDisplayComponent.RefreshAsync();
    }
}
