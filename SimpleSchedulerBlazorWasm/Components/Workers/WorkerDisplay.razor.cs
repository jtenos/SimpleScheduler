using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using SimpleSchedulerApiModels;
using SimpleSchedulerApiModels.Reply.Workers;
using SimpleSchedulerApiModels.Request.Workers;
using SimpleSchedulerServiceClient;

namespace SimpleSchedulerBlazorWasm.Components.Workers;

partial class WorkerDisplay
{
    [Inject]
    private NavigationManager Nav { get; set; } = default!;

    [Inject]
    private SweetAlertService Swal { get; set; } = default!;

    [Inject]
    private ServiceClient ServiceClient { get; set; } = default!;

    [Parameter]
    [EditorRequired]
    public WorkerWithSchedules? Worker { get; set; } = default!;

    [Parameter]
    [EditorRequired]
    public Worker[] AllWorkers { get; set; } = default!;

    [Parameter]
    [EditorRequired]
    public Pages.Workers WorkersComponent { get; set; } = default!;

    public async Task RefreshAsync()
    {
        (Error? error, GetWorkerReply? reply) = await ServiceClient.PostAsync<GetWorkerRequest, GetWorkerReply>(
            "Workers/GetWorker",
            new(Worker!.Worker.ID)
        );

        if (error is not null)
        {
            await Swal.FireAsync("Error", error.Message, SweetAlertIcon.Error);
            return;
        }

        Worker = reply!.Worker;
        StateHasChanged(); // TODO: Is this necessary?
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

        if (!string.IsNullOrEmpty(result.Value))
        {
            WorkersComponent.SetLoadingOn();
            (Error? error, _) = await ServiceClient.PostAsync<DeleteWorkerRequest, DeleteWorkerReply>(
                "Workers/DeleteWorker",
                new(Worker!.Worker.ID)
            );

            if (error is not null)
            {
                await Swal.FireAsync("Error", error.Message, SweetAlertIcon.Error);
                WorkersComponent.SetLoadingOff();
                return;
            }

            await WorkersComponent.RefreshAsync();
        }
    }

    private async Task ReactivateWorker()
    {
        WorkersComponent.SetLoadingOn();
        (Error? error, _) = await ServiceClient.PostAsync<ReactivateWorkerRequest, ReactivateWorkerReply>(
            "Workers/ReactivateWorker",
            new(Worker!.Worker.ID)
        );

        if (error is not null)
        {
            await Swal.FireAsync("Error", error.Message, SweetAlertIcon.Error);
            WorkersComponent.SetLoadingOff();
            return;
        }

        await WorkersComponent.RefreshAsync();
    }

    private async Task RunWorker()
    {
        WorkersComponent.SetLoadingOff();
        (Error? error, _) = await ServiceClient.PostAsync<RunNowRequest, RunNowReply>(
            "Workers/RunNow",
            new(Worker!.Worker.ID)
        );

        if (error is not null)
        {
            await Swal.FireAsync("Error", error.Message, SweetAlertIcon.Error);
            WorkersComponent.SetLoadingOff();
            return;
        }

        Nav.NavigateTo($"jobs/{Worker!.Worker.ID}");
    }

    private Task GoToJobs()
    {
        Nav.NavigateTo($"jobs/{Worker!.Worker.ID}");
        return Task.CompletedTask;
    }
}
