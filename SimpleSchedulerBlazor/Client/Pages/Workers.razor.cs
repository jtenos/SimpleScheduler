using Microsoft.AspNetCore.Components;
using SimpleScheduler.Blazor.Shared.ServiceContracts;
using SimpleSchedulerApiModels;
using SimpleSchedulerApiModels.Reply.Workers;
using SimpleSchedulerApiModels.Request.Workers;

namespace SimpleSchedulerBlazor.Client.Pages;

partial class Workers
{
    [Inject]
    private IWorkersService WorkersService { get; set; } = default!;

    public IEnumerable<Worker> WorkerList { get; set; } = Array.Empty<Worker>();

    protected override async Task OnInitializedAsync()
    {
        GetAllWorkersRequest request = new();
        GetAllWorkersReply reply = await WorkersService.GetAllWorkersAsync(request);
        WorkerList = reply.Workers.ToArray();
    }
}
