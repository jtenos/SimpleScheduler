using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using SimpleSchedulerApiModels;
using SimpleSchedulerApiModels.Reply.Jobs;
using SimpleSchedulerApiModels.Reply.Workers;
using SimpleSchedulerApiModels.Request.Jobs;
using SimpleSchedulerApiModels.Request.Workers;

namespace SimpleSchedulerBlazor.Client.Pages;

partial class Jobs
{
    private EditContext SearchEditContext { get; set; } = default!;
    private readonly SearchModel SearchCriteria = new();
    private Worker[] AllWorkers { get; set; } = Array.Empty<Worker>();

    private bool Loading { get; set; } = true;

    [Parameter]
    public long? WorkerID { get; set; }

    [Inject]
    private ServiceClient ServiceClient { get; set; } = default!;

    [Inject]
    private SweetAlertService Swal { get; set; } = default!;

    private JobWithWorkerID[] JobDetails { get; set; } = Array.Empty<JobWithWorkerID>();

    private readonly Dictionary<long, Worker> _allWorkersByID = new();

    protected override async Task OnInitializedAsync()
    {
        SearchEditContext = new(SearchCriteria);
        SearchEditContext.OnFieldChanged += async (sender, e) =>
        {
            await LoadJobsAsync();
            StateHasChanged();
        };
        await LoadWorkersAsync();
        await LoadJobsAsync();
        Loading = false;
    }

    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        return base.OnAfterRenderAsync(firstRender);
    }

    private async Task LoadWorkersAsync()
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

        AllWorkers = reply!.Workers
            .Select(w => w.Worker)
            .OrderBy(w => w.IsActive ? 0 : 1)
            .ThenBy(w => w.WorkerName)
            .ToArray();
        foreach (Worker worker in AllWorkers)
        {
            _allWorkersByID[worker.ID] = worker;
        }
    }

    private async Task LoadJobsAsync()
    {
        (Error? error, GetJobsReply? reply) = await ServiceClient.PostAsync<GetJobsRequest, GetJobsReply>(
            "Jobs/GetJobs",
            new GetJobsRequest(
                workerID: SearchCriteria.WorkerID,
                statusCode: SearchCriteria.StatusCode,
                pageNumber: SearchCriteria.PageNumber,
                overdueOnly: false
            )
        );

        if (error is not null)
        {
            await Swal.FireAsync("Error", error.Message, SweetAlertIcon.Error);
            return;
        }

        JobDetails = reply!.Jobs;
    }

    public class SearchModel
    {
        public long? WorkerID { get; set; }

        private string? _statusCode;
        public string? StatusCode
        {
            get => _statusCode; 
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    _statusCode = null;
                    return;
                }
                _statusCode = value;
            }
        }
        public int PageNumber { get; set; } = 1;
    }
}
