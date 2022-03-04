using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using SimpleSchedulerApiModels;
using SimpleSchedulerApiModels.Reply.Workers;
using SimpleSchedulerApiModels.Request.Workers;
using SimpleSchedulerBlazor.Client.Errors;

namespace SimpleSchedulerBlazor.Client.Components.Workers;

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

    private string? ParentWorkerName { get; set; }

    protected override Task OnInitializedAsync()
    {
        ParentWorkerName = AllWorkers.SingleOrDefault(w => w.ID == Worker?.Worker.ParentWorkerID)?.WorkerName;
        return Task.CompletedTask;
    }

    public async Task RefreshAsync()
    {
        (await ServiceClient.TryPostAsync<GetWorkerRequest, GetWorkerReply>(
            "Workers/GetWorker",
            new(Worker!.Worker.ID)
        )).Switch(
            (GetWorkerReply reply) =>
            {
                Worker = reply.Worker;
                StateHasChanged();
            },
            async (Error error) =>
            {
                await Swal.FireAsync("Error", error.Message, SweetAlertIcon.Error);
            }
        );
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

            (await ServiceClient.TryPostAsync<DeleteWorkerRequest, DeleteWorkerReply>(
                "Workers/DeleteWorker",
                new(Worker!.Worker.ID)
            )).Switch(
                async (DeleteWorkerReply reply) =>
                {
                    await WorkersComponent.RefreshAsync();
                },
                async (Error error) =>
                {
                    await Swal.FireAsync("Error", error.Message, SweetAlertIcon.Error);
                    WorkersComponent.SetLoadingOff();
                }
            );
        }
    }

    private async Task ReactivateWorker()
    {
        WorkersComponent.SetLoadingOn();

        (await ServiceClient.TryPostAsync<ReactivateWorkerRequest, ReactivateWorkerReply>(
            "Workers/ReactivateWorker",
            new(Worker!.Worker.ID)
        )).Switch(
            async (ReactivateWorkerReply reply) =>
            {
                await WorkersComponent.RefreshAsync();
            },
            async (Error error) =>
            {
                await Swal.FireAsync("Error", error.Message, SweetAlertIcon.Error);
                WorkersComponent.SetLoadingOff();
            }
        );
    }

    private Task RunWorker()
    {
        return Task.CompletedTask;
    }
}
