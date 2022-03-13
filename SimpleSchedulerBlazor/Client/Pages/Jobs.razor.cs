using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using SimpleSchedulerApiModels;
using SimpleSchedulerApiModels.Reply.Jobs;
using SimpleSchedulerApiModels.Reply.Workers;
using SimpleSchedulerApiModels.Request.Jobs;
using SimpleSchedulerApiModels.Request.Workers;
using SimpleSchedulerServiceClient;

namespace SimpleSchedulerBlazor.Client.Pages;

partial class Jobs
{
    private EditContext SearchEditContext { get; set; } = default!;
    private readonly SearchModel SearchCriteria = new();
    private Worker[] AllWorkers { get; set; } = Array.Empty<Worker>();

    public bool Loading { get; set; } = true;

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
                WorkerID: SearchCriteria.WorkerID,
                StatusCode: SearchCriteria.StatusCode,
                PageNumber: SearchCriteria.PageNumber,
                OverdueOnly: false
            )
        );

        if (error is not null)
        {
            await Swal.FireAsync("Error", error.Message, SweetAlertIcon.Error);
            return;
        }

        JobDetails = reply!.Jobs;

        // Make a second search, only for ERR records. These may already be in the main results,
        // so only add them to the list if they are not included.
        (error, reply) = await ServiceClient.PostAsync<GetJobsRequest, GetJobsReply>(
            "Jobs/GetJobs",
            new GetJobsRequest(
                WorkerID: null,
                StatusCode: "ERR",
                PageNumber: 1,
                OverdueOnly: false
            )
        );

        if (error is not null)
        {
            await Swal.FireAsync("Error", error.Message, SweetAlertIcon.Error);
            return;
        }

        if (!reply!.Jobs.Any()) { return; }

        HashSet<long> resultsIDs = new(JobDetails.Select(jd => jd.ID));
        JobWithWorkerID[] errorJobs = reply!.Jobs
            .Where(j => !resultsIDs.Contains(j.ID))
            .ToArray();

        if (errorJobs.Any())
        {
            JobDetails = JobDetails.Concat(errorJobs).ToArray();
        }

        JobDetails = JobDetails
            .OrderBy(x => x.StatusCode == "ERR" ? 0 : 1)
            .ThenByDescending(x => x.QueueDateUTC)
            .ToArray();
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
