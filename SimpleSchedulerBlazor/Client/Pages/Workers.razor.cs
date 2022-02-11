using Microsoft.AspNetCore.Components;
using SimpleSchedulerModels;
using System.Net.Http.Json;

namespace SimpleSchedulerBlazor.Client.Pages;

partial class Workers
{
    [Inject]
    private HttpClient Http { get; set; } = default!;

    public IEnumerable<WorkerDetail> WorkerList { get; set; } = Array.Empty<WorkerDetail>();

    protected override async Task OnInitializedAsync()
    {
        WorkerList = await Http.GetFromJsonAsync<IEnumerable<WorkerDetail>>("Workers/GetAllWorkers")
            ?? Array.Empty<WorkerDetail>();
    }
}
