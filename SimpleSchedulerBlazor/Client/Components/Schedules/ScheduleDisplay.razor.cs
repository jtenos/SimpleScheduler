using CurrieTechnologies.Razor.SweetAlert2;
using Grpc.Core;
using Microsoft.AspNetCore.Components;
using SimpleScheduler.Blazor.Shared.ServiceContracts;
using SimpleSchedulerApiModels;
using SimpleSchedulerBlazor.Client.Components.Workers;

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
    private ISchedulesService ScheduleService { get; set; } = default!;

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

        string message;
        if (!string.IsNullOrEmpty(result.Value))
        {
            IsLoading = true;
            try
            {
                await ScheduleService.DeleteScheduleAsync(new(Schedule.ID));
                await WorkerDisplayComponent.RefreshAsync();
                return;
            }
            catch (RpcException ex)
            {
                message = ex.Status.Detail;
                if (string.IsNullOrWhiteSpace(message))
                {
                    message = ex.Message;
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            await Swal.FireAsync("Error", message, SweetAlertIcon.Error);
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
