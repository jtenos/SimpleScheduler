using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using SimpleScheduler.Blazor.Shared.ServiceContracts;
using SimpleSchedulerApiModels;
using SimpleSchedulerApiModels.Reply.Workers;
using SimpleSchedulerApiModels.Request.Workers;
using SimpleSchedulerBlazor.Client.Components;
using Wws = SimpleSchedulerApiModels.WorkerWithSchedules;

namespace SimpleSchedulerBlazor.Client.Pages;

partial class Workers
{
    [Inject]
    private IWorkersService WorkersService { get; set; } = default!;

    private AsYouTypeInputText? SearchTextBox { get; set; } = default!;

    private readonly SearchModel SearchCriteria = new();
    private EditContext SearchEditContext { get; set; } = default!;

    private (Wws Worker1, Wws? Worker2, Wws? Worker3)[] WorkerGroups { get; set; } = default!;
    private Wws[] AllWorkersWithSchedules { get; set; } = default!;
    private Worker[] AllWorkers => AllWorkersWithSchedules.Select(w => w.Worker).ToArray();

    private bool Loading { get; set; } = true;

    protected override async Task OnInitializedAsync()
    {
        GetAllWorkersRequest request = new();
        GetAllWorkersReply reply = await WorkersService.GetAllWorkersAsync(request);
        AllWorkersWithSchedules = reply.Workers;

        SearchEditContext = new(SearchCriteria);
        SearchEditContext.OnFieldChanged += (sender, e) =>
        {
            SetFilteredWorkerGroups();
        };

        SetFilteredWorkerGroups();
        Loading = false;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        Console.WriteLine("OnAfterRenderAsync({0}): SearchTextBox?.Element.HasValue={1}", 
            firstRender, SearchTextBox?.Element.HasValue);
        try
        {
            if (SearchTextBox?.Element.HasValue == true)
            {
                await SearchTextBox.Element.Value.FocusAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception firstRender={0}: {1}", firstRender, ex);
        }
    }

    private void SetFilteredWorkerGroups()
    {
        Wws[] filteredWorkers = AllWorkersWithSchedules.Where(IsSearchMatch).ToArray();

        List<(Wws, Wws?, Wws?)> workerGroups = new();
        for (int i = 0; i < filteredWorkers.Length; i += 3)
        {
            Wws worker1 = filteredWorkers[i];
            Wws? worker2 = null;
            if (i < filteredWorkers.Length - 2)
            {
                worker2 = filteredWorkers[i + 1];
            }
            Wws? worker3 = null;
            if (i < filteredWorkers.Length - 3)
            {
                worker3 = filteredWorkers[i + 2];
            }
            workerGroups.Add((worker1, worker2, worker3));
        }
        WorkerGroups = workerGroups.ToArray();
        StateHasChanged();
    }

    private bool IsSearchMatch(Wws w)
    {
        string searchText = SearchCriteria.SearchText;
        return w.Worker.WorkerName.Contains(searchText, StringComparison.InvariantCultureIgnoreCase)
            || w.Worker.DetailedDescription.Contains(searchText, StringComparison.InvariantCultureIgnoreCase)
            || w.Worker.DirectoryName.Contains(searchText, StringComparison.InvariantCultureIgnoreCase)
            || w.Worker.Executable.Contains(searchText, StringComparison.InvariantCultureIgnoreCase)
            || w.Worker.ArgumentValues.Contains(searchText, StringComparison.InvariantCultureIgnoreCase);
    }

    public class SearchModel
    {
        public string SearchText { get; set; } = "";
    }
}
