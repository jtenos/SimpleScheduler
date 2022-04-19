using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using SimpleSchedulerApiModels;
using SimpleSchedulerApiModels.Reply.Workers;
using SimpleSchedulerApiModels.Request.Workers;
using SimpleSchedulerServiceClient;

namespace SimpleSchedulerBlazorWasm.Pages;

partial class WorkerEdit
{
    [Inject]
    private ServiceClient ServiceClient { get; set; } = default!;

    [Inject]
    private NavigationManager Nav { get; set; } = default!;


    [Inject]
    private SweetAlertService Swal { get; set; } = default!;

    [Parameter]
    public long ID { get; set; }

    private bool Loading { get; set; } = true;
    private Worker Worker { get; set; } = new();
    private WorkerIDName[] AllWorkers { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        await LoadWorkerAsync();

        await LoadActiveWorkersAsync();

        Loading = false;
    }

    private async Task LoadActiveWorkersAsync()
    {
        (Error? error, GetAllActiveWorkerIDNamesReply? reply) = await ServiceClient.PostAsync<GetAllActiveWorkerIDNamesRequest, GetAllActiveWorkerIDNamesReply>(
            "Workers/GetAllActiveWorkerIDNames",
            new GetAllActiveWorkerIDNamesRequest()
        );

        if (error is not null)
        {
            await Swal.FireAsync("Error", error.Message, SweetAlertIcon.Error);
            return;
        }

        AllWorkers = reply!.Workers;
    }

    private async Task LoadWorkerAsync()
    {
        (Error? error, GetWorkerReply? reply) = await ServiceClient.PostAsync<GetWorkerRequest, GetWorkerReply>(
            "Workers/GetWorker",
            new GetWorkerRequest(ID)
        );

        if (error is not null)
        {
            await Swal.FireAsync("Error", error.Message, SweetAlertIcon.Error);
            return;
        }

        Worker = reply!.Worker.Worker;
    }

    private async Task SaveWorker()
    {
        if (Worker.ID > 0)
        {
            (Error? error, _) = await ServiceClient.PostAsync<UpdateWorkerRequest, UpdateWorkerReply>(
                "Workers/UpdateWorker",
                new UpdateWorkerRequest(
                    ID: Worker.ID,
                    WorkerName: Worker.WorkerName,
                    DetailedDescription: Worker.DetailedDescription,
                    EmailOnSuccess: Worker.EmailOnSuccess,
                    ParentWorkerID: Worker.ParentWorkerID,
                    TimeoutMinutes: Worker.TimeoutMinutes,
                    DirectoryName: Worker.DirectoryName,
                    Executable: Worker.Executable,
                    ArgumentValues: Worker.ArgumentValues
                )
            );

            if (error is not null)
            {
                await Swal.FireAsync("Error", error.Message, SweetAlertIcon.Error);
                return;
            }

            Nav.NavigateTo("workers");
        }
    }
}
