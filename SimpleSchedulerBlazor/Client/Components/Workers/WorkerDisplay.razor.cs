using CurrieTechnologies.Razor.SweetAlert2;
using Grpc.Core;
using Microsoft.AspNetCore.Components;
using SimpleScheduler.Blazor.Shared.ServiceContracts;
using SimpleSchedulerApiModels;

namespace SimpleSchedulerBlazor.Client.Components.Workers;

partial class WorkerDisplay
{
    [Inject]
    private NavigationManager Nav { get; set; } = default!;

    [Inject]
    private SweetAlertService Swal { get; set; } = default!;

    [Inject]
    private IWorkersService WorkerService { get; set; } = default!;

    [Parameter]
    [EditorRequired]
    public WorkerWithSchedules? Worker { get; set; } = default!;

    [Parameter]
    [EditorRequired]
    public Worker[] AllWorkers { get; set; } = default!;

    [Parameter]
    [EditorRequired]
    public Pages.Workers WorkersComponent { get; set; } = default!;

    private string? ParentWorkerName { get; set; }

    protected override Task OnInitializedAsync()
    {
        ParentWorkerName = AllWorkers.SingleOrDefault(w => w.ID == Worker?.Worker.ParentWorkerID)?.WorkerName;
        return Task.CompletedTask;
    }

    public async Task RefreshAsync()
    {
        Worker = (await WorkerService.GetWorkerAsync(new(Worker!.Worker.ID))).Worker;
        StateHasChanged();
    }

    private Task EditWorker()
    {
        Nav.NavigateTo($"workers/{Worker!.Worker.ID}");
        return Task.CompletedTask;
    }

    private async Task DeleteWorker()
    {
        SweetAlertResult result = await Swal.FireAsync(new SweetAlertOptions
        {
            Title = "Are you sure?",
            Text = "This worker will be deactivated, along with any schedules",
            Icon = SweetAlertIcon.Warning,
            ShowCancelButton = true,
            ConfirmButtonText = "Delete",
            CancelButtonText = "Cancel"
        });

        string message;
        if (!string.IsNullOrEmpty(result.Value))
        {
            WorkersComponent.SetLoadingOn();
            try
            {
                await WorkerService.DeleteWorkerAsync(new(Worker!.Worker.ID));
                await WorkersComponent.RefreshAsync();
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
            WorkersComponent.SetLoadingOff();
        }
    }

    private async Task ReactivateWorker()
    {
        string message;
        WorkersComponent.SetLoadingOn();
        try
        {
            await WorkerService.ReactivateWorkerAsync(new(Worker!.Worker.ID));
            await WorkersComponent.RefreshAsync();
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
        WorkersComponent.SetLoadingOff();
    }

    private Task RunWorker()
    {
        return Task.CompletedTask;
    }
}
