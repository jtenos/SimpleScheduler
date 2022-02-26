using Microsoft.AspNetCore.Components;
using SimpleScheduler.Blazor.Shared.ServiceContracts;
using SimpleSchedulerApiModels;
using SimpleSchedulerApiModels.Reply.Workers;
using SimpleSchedulerApiModels.Request.Workers;
using Wws = SimpleSchedulerApiModels.WorkerWithSchedules;

namespace SimpleSchedulerBlazor.Client.Pages;

partial class Workers
{
    [Inject]
    private IWorkersService WorkersService { get; set; } = default!;

    private (Wws Worker1, Wws? Worker2, Wws? Worker3)[] WorkerGroups { get; set; } = default!;
    private Worker[] AllWorkers { get; set; } = default!;

    private bool Loading { get; set; } = true;

    protected override async Task OnInitializedAsync()
    {
        GetAllWorkersRequest request = new();
        GetAllWorkersReply reply = await WorkersService.GetAllWorkersAsync(request);

        AllWorkers = reply.Workers.Select(w => w.Worker).ToArray();

        List<(Wws, Wws?, Wws?)> workerGroups = new();
        for (int i = 0; i < reply.Workers.Length; i += 3)
        {
            WorkerWithSchedules worker1 = reply.Workers[i];
            WorkerWithSchedules? worker2 = null;
            if (i < reply.Workers.Length - 2)
            {
                worker2 = reply.Workers[i + 1];
            }
            WorkerWithSchedules? worker3 = null;
            if (i < reply.Workers.Length - 3)
            {
                worker3 = reply.Workers[i + 2];
            }
            workerGroups.Add((worker1, worker2, worker3));
        }
        WorkerGroups = workerGroups.ToArray();
        Loading = false;
    }
}
