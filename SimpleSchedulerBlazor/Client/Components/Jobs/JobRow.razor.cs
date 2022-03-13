using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SimpleSchedulerApiModels;
using SimpleSchedulerApiModels.Reply.Jobs;
using SimpleSchedulerApiModels.Request.Jobs;
using SimpleSchedulerServiceClient;
using Timer = System.Timers.Timer;

namespace SimpleSchedulerBlazor.Client.Components.Jobs;

partial class JobRow
{
    [Parameter]
    [EditorRequired]
    public Job Job { get; set; } = default!;

    [Parameter]
    [EditorRequired]
    public Worker Worker { get; set; } = default!;

    [Parameter]
    [EditorRequired]
    public Worker? ParentWorker { get; set; }

    [Parameter]
    [EditorRequired]
    public Pages.Jobs JobsComponent { get; set; } = default!;

    private bool Loading { get; set; } = true;

    [Inject]
    private ServiceClient ServiceClient { get; set; } = default!;

    [Inject]
    private SweetAlertService Swal { get; set; } = default!;

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    protected override Task OnInitializedAsync()
    {
        Loading = false;

        if (Job.StatusCode == "RUN")
        {
            // Job is currently running - we'll ping status every five seconds
            StartFiveSecondTimer();
        }
        else if (Job.StatusCode == "NEW")
        {
            // Job is not running yet. If it's about to run or already should have started, then we'll
            // just start the 5-second timer. Otherwise we will wait until it's time for the job to start,
            // then start the 5-second timer.
            double millsecondsUntilStart = Job.QueueDateUTC.Subtract(DateTime.UtcNow).TotalMilliseconds;
            if (millsecondsUntilStart <= 5000)
            {
                StartFiveSecondTimer();
            }
            else
            {
                Timer timer = new(interval: millsecondsUntilStart);
                timer.Elapsed += (sender, e) =>
                {
                    timer.Stop();
                    timer.Dispose();
                    StartFiveSecondTimer();
                };
            }
        }

        return Task.CompletedTask;
    }

    private void StartFiveSecondTimer()
    {
        Timer timer = new(interval: TimeSpan.FromSeconds(5).TotalMilliseconds);
        timer.Elapsed += async (sender, e) =>
        {
            timer.Stop();
            await ReloadJobAsync();
            if (Job.StatusCode != "RUN")
            {
                timer.Dispose();
                return;
            }
            timer.Start();
        };
        timer.Start();
    }

    private async Task ReloadJobAsync()
    {
        Loading = true;
        StateHasChanged();
        (Error? error, GetJobReply? reply) = await ServiceClient.PostAsync<GetJobRequest, GetJobReply>(
            "Jobs/GetJob",
            new(Job.ID)
        );

        if (error is not null)
        {
            Console.WriteLine(error);
            //await Swal.FireAsync("Error", error.Message, SweetAlertIcon.Error);
            Loading = false;
            return;
        }

        Job = reply!.Job;
        Loading = false;
        StateHasChanged();
    }

    private async Task ViewDetailedMessage()
    {
        JobsComponent.SetLoadingOn();
        (Error? error, GetDetailedMessageReply? reply) = await ServiceClient.PostAsync<GetDetailedMessageRequest, GetDetailedMessageReply>(
            "Jobs/GetDetailedMessage",
            new(Job.ID)
        );

        if (error is not null)
        {
            await Swal.FireAsync("Error", error.Message, SweetAlertIcon.Error);
            return;
        }

        await JSRuntime.InvokeVoidAsync("Jobs.viewDetailedMessage",
            Worker.WorkerName, reply!.DetailedMessage);
        JobsComponent.SetLoadingOff();
    }

    private async Task AcknowledgeError()
    {
        Loading = true;
        (Error? error, _) = await ServiceClient.PostAsync<AcknowledgeErrorRequest, AcknowledgeErrorReply>(
            "Jobs/AcknowledgeError",
            new(Job.AcknowledgementCode)
        );

        if (error is not null)
        {
            await Swal.FireAsync("Error", error.Message, SweetAlertIcon.Error);
            Loading = false;
            return;
        }

        await ReloadJobAsync();
    }

    private async Task CancelJob()
    {
        SweetAlertResult result = await Swal.FireAsync(new SweetAlertOptions
        {
            Title = "Are you sure?",
            Text = "This job will be cancelled, and will not run again until the next schedule",
            Icon = SweetAlertIcon.Warning,
            ShowCancelButton = true,
            ConfirmButtonText = "Cancel job",
            CancelButtonText = "Do not cancel job"
        });

        if (!string.IsNullOrEmpty(result.Value))
        {
            Loading = true;
            (Error? error, _) = await ServiceClient.PostAsync<CancelJobRequest, CancelJobReply>(
                "Jobs/CancelJob",
                new(Job.ID)
            );

            if (error is not null)
            {
                await Swal.FireAsync("Error", error.Message, SweetAlertIcon.Error);
                Loading = false;
                return;
            }

            await ReloadJobAsync();
        }
    }
}
