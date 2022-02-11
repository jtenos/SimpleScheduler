using SimpleSchedulerModels;

namespace SimpleSchedulerBlazor.Client.Pages;

partial class Workers
{
    public IEnumerable<Worker> WorkerList { get; set; } = Array.Empty<Worker>();

    protected override async Task OnInitializedAsync()
    {
        WorkerList = new[]
        {
            new Worker(1, true, "First Name", "First Detail", "", null, 20, "First Dir", "First Exe", "First Args"),
            new Worker(2, true, "Second Name", "Second Detail", "", null, 20, "Second Dir", "Second Exe", "Second Args"),
        };
        await Task.CompletedTask;
    }
}
