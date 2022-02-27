using CurrieTechnologies.Razor.SweetAlert2;
using Grpc.Core;
using Microsoft.AspNetCore.Components;
using SimpleScheduler.Blazor.Shared.ServiceContracts;
using SimpleSchedulerApiModels;
using SimpleSchedulerApiModels.Request.Workers;

namespace SimpleSchedulerBlazor.Client.Pages;

partial class WorkerEdit
{
    [Inject]
    private IWorkersService WorkersService { get; set; } = default!;

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
        Worker = (await WorkersService.GetWorkerAsync(new(ID))).Worker;
        AllWorkers = (await WorkersService.GetAllActiveWorkerIDNamesAsync(new())).Workers.ToArray();
        Loading = false;
    }

    private async Task SaveWorker()
    {
        try
        {
            if (Worker.ID > 0)
            {
                await WorkersService.UpdateWorkerAsync(new UpdateWorkerRequest(
                    id: Worker.ID,
                    workerName: Worker.DirectoryName,
                    detailedDescription: Worker.DetailedDescription,
                    emailOnSuccess: Worker.EmailOnSuccess,
                    parentWorkerID: Worker.ParentWorkerID,
                    timeoutMinutes: Worker.TimeoutMinutes,
                    directoryName: Worker.DirectoryName,
                    executable: Worker.Executable,
                    argumentValues: Worker.ArgumentValues
                ));
                Nav.NavigateTo("workers");
            }
        }
        catch (RpcException ex)
        {
            string message = ex.Status.Detail;
            if (string.IsNullOrWhiteSpace(message))
            {
                message = ex.Message;
            }
            await Swal.FireAsync(
                title: "Error saving worker",
                message: message,
                icon: SweetAlertIcon.Error
            );
        }
        catch (Exception ex)
        {
            await Swal.FireAsync(
                title: "Error saving worker",
                message: ex.Message,
                icon: SweetAlertIcon.Error
            );
        }
    }
}
