using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using SimpleSchedulerApiModels;
using SimpleSchedulerApiModels.Reply.Jobs;
using SimpleSchedulerApiModels.Reply.Schedules;
using SimpleSchedulerApiModels.Reply.Workers;
using SimpleSchedulerApiModels.Request.Jobs;
using SimpleSchedulerApiModels.Request.Schedules;
using SimpleSchedulerApiModels.Request.Workers;

namespace SimpleSchedulerBlazor.Client.Pages;

partial class Jobs
{
    private readonly SearchModel SearchCriteria = new();
    private Worker[] AllWorkers { get; set; } = Array.Empty<Worker>();

    private bool Loading { get; set; } = true;

    [Parameter]
    public long? WorkerID { get; set; }

    [Inject]
    private ServiceClient ServiceClient { get; set; } = default!;

    [Inject]
    private SweetAlertService Swal { get; set; } = default!;

    private (Job Job, Schedule Schedule, Worker Worker)[] JobDetails { get; set; } = Array.Empty<(Job Job, Schedule Schedule, Worker Worker)>();

    private readonly Dictionary<long, Worker> _allWorkersByID = new();

    protected override async Task OnInitializedAsync()
    {
        Console.WriteLine($"OnInitializedAsync-start {JobDetails.Length} jobs in collection");
        Console.WriteLine("OnInitializedAsync-start");
        await LoadWorkersAsync();
        await LoadJobsAsync();
        Loading = false;
        Console.WriteLine($"OnInitializedAsync-end {JobDetails.Length} jobs in collection");
        Console.WriteLine("OnInitializedAsync-end");
    }

    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        Console.WriteLine($"OnAfterRenderAsync {JobDetails.Length} jobs in collection");
        Console.WriteLine("OnAfterRenderAsync");
        return base.OnAfterRenderAsync(firstRender);
    }

    private async Task LoadWorkersAsync()
    {
        Console.WriteLine($"LoadWorkersAsync-start {JobDetails.Length} jobs in collection");

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
        Console.WriteLine($"LoadWorkersAsync-end {JobDetails.Length} jobs in collection");
    }

    private async Task LoadJobsAsync()
    {
        Console.WriteLine($"LoadJobsAsync-start {JobDetails.Length} jobs in collection");

        (Error? error, GetJobsReply? reply) = await ServiceClient.PostAsync<GetJobsRequest, GetJobsReply>(
            "Jobs/GetJobs",
            new GetJobsRequest(
                workerID: WorkerID,
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

        Job[] jobs = reply!.Jobs;
        long[] jobIDs = jobs.Select(j => j.ID).ToArray();

        HashSet<long> scheduleIDs = new(jobs.Select(j => j.ScheduleID));
        Dictionary<long, Schedule> schedulesByID = (await GetSchedulesAsync(scheduleIDs))
            .ToDictionary(s => s.ID, s => s);

        var list = new List<(Job Job, Schedule Schedule, Worker Worker)>();
        foreach (Job job in jobs)
        {
            Schedule schedule = schedulesByID[job.ScheduleID];
            Worker worker = _allWorkersByID[schedule.WorkerID];
            list.Add((job, schedule, worker));
        }
        JobDetails = list.ToArray();
        //StateHasChanged();

        Console.WriteLine($"LoadJobsAsync-end {JobDetails.Length} jobs in collection");
    }

    private async Task<Schedule[]> GetSchedulesAsync(HashSet<long> scheduleIDs)
    {
        (Error? error, GetSchedulesReply? reply) = await ServiceClient.PostAsync<GetSchedulesRequest, GetSchedulesReply>(
            "Schedules/GetSchedules",
            new GetSchedulesRequest(
                ids: scheduleIDs.ToArray()
            )
        );

        if (error is not null)
        {
            await Swal.FireAsync("Error", error.Message, SweetAlertIcon.Error);
            return Array.Empty<Schedule>();
        }

        return reply!.Schedules;
    }

    public class SearchModel
    {
        public long? WorkerID { get; set; }
        public string? StatusCode { get; set; }
        public int PageNumber { get; set; } = 1;
    }
}
