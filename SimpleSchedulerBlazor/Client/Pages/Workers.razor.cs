using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using SimpleSchedulerApiModels;
using SimpleSchedulerApiModels.Reply.Workers;
using SimpleSchedulerApiModels.Request.Workers;
using SimpleSchedulerBlazor.Client.Components;
using Wws = SimpleSchedulerApiModels.WorkerWithSchedules;

namespace SimpleSchedulerBlazor.Client.Pages;

partial class Workers
{
    [Inject]
    private ServiceClient ServiceClient { get; set; } = default!;

    [Inject]
    private SweetAlertService Swal { get; set; } = default!;

    private AsYouTypeInputText? SearchTextBox { get; set; } = default!;

    private readonly SearchModel SearchCriteria = new();
    private EditContext SearchEditContext { get; set; } = default!;

    private (Wws Worker1, Wws? Worker2, Wws? Worker3)[] WorkerGroups { get; set; } = default!;
    private Wws[] AllWorkersWithSchedules { get; set; } = default!;
    private Worker[] AllWorkers => AllWorkersWithSchedules.Select(w => w.Worker).ToArray();

    private bool Loading { get; set; } = true;

    protected override async Task OnInitializedAsync()
    {
        SearchEditContext = new(SearchCriteria);
        SearchEditContext.OnFieldChanged += (sender, e) =>
        {
            SetFilteredWorkerGroups();
        };

        await LoadGroupsAsync();
    }

    private async Task LoadGroupsAsync()
    {
        (Error? error, GetAllWorkersReply? reply) = await ServiceClient.PostAsync<GetAllWorkersRequest, GetAllWorkersReply>(
            "Workers/GetAllWorkers",
            new GetAllWorkersRequest()
        );

        if (error is not null)
        {
            await Swal.FireAsync("Error", error.Message, SweetAlertIcon.Error);
            return;
        }

        AllWorkersWithSchedules = reply!.Workers;
        Array.Sort(AllWorkersWithSchedules, (w1, w2) => w1.Worker.WorkerName.CompareTo(w2.Worker.WorkerName));
        SetFilteredWorkerGroups();
        Loading = false;
        StateHasChanged(); // TODO: Is this necessary?
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        try
        {
            if (SearchTextBox?.Element.HasValue == true && !Loading)
            {
                await SearchTextBox.Element.Value.FocusAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception firstRender={0}: {1}", firstRender, ex);
        }
    }

    public void SetLoadingOn()
    {
        Loading = true;
        StateHasChanged();
    }

    public void SetLoadingOff()
    {
        Loading = false;
        StateHasChanged();
    }

    public async Task RefreshAsync()
    {
        Loading = true;
        await LoadGroupsAsync();
    }

    private void SetFilteredWorkerGroups()
    {
        Wws[] filteredWorkers = AllWorkersWithSchedules.Where(IsSearchMatch).ToArray();

        List<(Wws, Wws?, Wws?)> workerGroups = new();
        for (int i = 0; i < filteredWorkers.Length; i += 3)
        {
            Wws worker1 = filteredWorkers[i];
            Wws? worker2 = null;
            if (i < filteredWorkers.Length - 1)
            {
                worker2 = filteredWorkers[i + 1];
            }
            Wws? worker3 = null;
            if (i < filteredWorkers.Length - 2)
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
        if (w.Worker.IsActive && SearchCriteria.ActiveType != SearchModel.ACTIVE) { return false; }
        if (!w.Worker.IsActive && SearchCriteria.ActiveType != SearchModel.INACTIVE) { return false; }

        string searchText = SearchCriteria.SearchText;
        return w.Worker.WorkerName.Contains(searchText, StringComparison.InvariantCultureIgnoreCase)
            || w.Worker.DetailedDescription.Contains(searchText, StringComparison.InvariantCultureIgnoreCase)
            || w.Worker.DirectoryName.Contains(searchText, StringComparison.InvariantCultureIgnoreCase)
            || w.Worker.Executable.Contains(searchText, StringComparison.InvariantCultureIgnoreCase)
            || w.Worker.ArgumentValues.Contains(searchText, StringComparison.InvariantCultureIgnoreCase);
    }

    public class SearchModel
    {
        public const string ACTIVE = "ACTIVE";
        public const string INACTIVE = "INACTIVE";

        public string SearchText { get; set; } = "";
        public string ActiveType { get; set; } = ACTIVE;
    }
}
