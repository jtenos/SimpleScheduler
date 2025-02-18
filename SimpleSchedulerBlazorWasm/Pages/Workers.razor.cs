﻿using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using SimpleSchedulerApiModels;
using SimpleSchedulerApiModels.Reply.Workers;
using SimpleSchedulerApiModels.Request.Workers;
using SimpleSchedulerBlazorWasm.Components;
using SimpleSchedulerServiceClient;
using Wws = SimpleSchedulerApiModels.WorkerWithSchedules;

namespace SimpleSchedulerBlazorWasm.Pages;

partial class Workers
{
    [Inject]
    private ServiceClient ServiceClient { get; set; } = default!;

    [Inject]
    private SweetAlertService Swal { get; set; } = default!;

    [Inject]
    private NavigationManager Nav { get; set; } = default!;

    [Parameter]
    public long? WorkerID { get; set; }

    private AsYouTypeInputText? SearchTextBox { get; set; } = default!;

    private readonly SearchModel _searchCriteria = new();
    private SearchModel SearchCriteria
    {
        get
        {
            _searchCriteria.WorkerID = WorkerID;
            return _searchCriteria;
        }
    }

    private EditContext SearchEditContext { get; set; } = default!;

    private Wws[] FilteredWorkers { get; set; } = default!;
    private Wws[] AllWorkersWithSchedules { get; set; } = default!;
    private Worker[] AllWorkers => AllWorkersWithSchedules.Select(w => w.Worker).ToArray();

    private bool Loading { get; set; } = true;

    protected override async Task OnInitializedAsync()
    {
        Console.WriteLine("Workers OnInitializedAsync");
        SearchEditContext = new(SearchCriteria);
        SearchEditContext.OnFieldChanged += (sender, e) =>
        {
            SetFilteredWorkerGroups();
        };
        await Task.CompletedTask;
    }

    protected override async Task OnParametersSetAsync()
    {
		await LoadGroupsAsync();
		SetFilteredWorkerGroups();
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
        FilteredWorkers = AllWorkersWithSchedules.Where(IsSearchMatch).ToArray();

        StateHasChanged();
    }

    private bool IsSearchMatch(Wws w)
    {
        if (w.Worker.IsActive && SearchCriteria.ActiveType != SearchModel.ACTIVE) { return false; }
        if (!w.Worker.IsActive && SearchCriteria.ActiveType != SearchModel.INACTIVE) { return false; }

        if (SearchCriteria.WorkerID.HasValue)
        {
            return w.Worker.ID == SearchCriteria.WorkerID;
        }

        string searchText = SearchCriteria.SearchText;
        return w.Worker.WorkerName.Contains(searchText, StringComparison.InvariantCultureIgnoreCase)
            || w.Worker.DetailedDescription.Contains(searchText, StringComparison.InvariantCultureIgnoreCase)
            || w.Worker.DirectoryName.Contains(searchText, StringComparison.InvariantCultureIgnoreCase)
            || w.Worker.Executable.Contains(searchText, StringComparison.InvariantCultureIgnoreCase)
            || w.Worker.ArgumentValues.Contains(searchText, StringComparison.InvariantCultureIgnoreCase);
    }

    private Task CreateWorker()
    {
        Nav.NavigateTo($"workers/0");
        return Task.CompletedTask;
    }

    public class SearchModel
    {
        public const string ACTIVE = "ACTIVE";
        public const string INACTIVE = "INACTIVE";

        private string _searchText = "";
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; WorkerID = null; }
        }
        public string ActiveType { get; set; } = ACTIVE;
        public long? WorkerID { get; set; }
    }
}
