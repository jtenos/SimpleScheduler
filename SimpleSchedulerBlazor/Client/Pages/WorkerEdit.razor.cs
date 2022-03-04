using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using SimpleSchedulerApiModels;
using SimpleSchedulerApiModels.Reply.Workers;
using SimpleSchedulerApiModels.Request.Workers;
using SimpleSchedulerBlazor.Client.Errors;

namespace SimpleSchedulerBlazor.Client.Pages;

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
        (await ServiceClient.TryPostAsync<GetWorkerRequest, GetWorkerReply>(
            "Workers/GetWorker",
            new GetWorkerRequest(ID)
        )).Switch(
            (GetWorkerReply reply) =>
            {
                Worker = reply.Worker.Worker;
            },
            async (Error error) =>
            {
                await Swal.FireAsync("Error", error.Message, SweetAlertIcon.Error);
            }
        );

        (await ServiceClient.TryPostAsync<GetAllActiveWorkerIDNamesRequest, GetAllActiveWorkerIDNamesReply>(
            "Workers/GetAllActiveWorkerIDNames",
            new GetAllActiveWorkerIDNamesRequest()
        )).Switch(
            (GetAllActiveWorkerIDNamesReply reply) =>
            {
                AllWorkers = reply.Workers;
            },
            async (Error error) =>
            {
                await Swal.FireAsync("Error", error.Message, SweetAlertIcon.Error);
            }
        );

        Loading = false;
    }

    private async Task SaveWorker()
    {
        if (Worker.ID > 0)
        {
            (await ServiceClient.TryPostAsync<UpdateWorkerRequest, UpdateWorkerReply>(
                "Workers/UpdateWorker",
                new UpdateWorkerRequest(
                    id: Worker.ID,
                    workerName: Worker.WorkerName,
                    detailedDescription: Worker.DetailedDescription,
                    emailOnSuccess: Worker.EmailOnSuccess,
                    parentWorkerID: Worker.ParentWorkerID,
                    timeoutMinutes: Worker.TimeoutMinutes,
                    directoryName: Worker.DirectoryName,
                    executable: Worker.Executable,
                    argumentValues: Worker.ArgumentValues
                )
            )).Switch(
                (UpdateWorkerReply reply) =>
                {
                    Nav.NavigateTo("workers");
                },
                async (Error error) =>
                {
                    await Swal.FireAsync("Error", error.Message, SweetAlertIcon.Error);
                }
            );
        }
    }
}
