using SimpleSchedulerApiModels;
using SimpleSchedulerApiModels.Reply.Jobs;
using SimpleSchedulerApiModels.Request.Jobs;
using SimpleSchedulerServiceClient;

namespace SimpleSchedulerService;

public sealed class JobExecutor
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<JobExecutor> _logger;
    private readonly IConfiguration _config;
    private readonly ServiceClient _serviceClient;

    public JobExecutor(
        IServiceProvider serviceProvider,
        ILogger<JobExecutor> logger,
        IConfiguration config,
        ServiceClient serviceClient)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _config = config;
        _serviceClient = serviceClient;
    }

    public async Task RestartStuckAppsAsync()
    {
        await _serviceClient.PostAsync<RestartStuckJobsRequest, RestartStuckJobsReply>(
            "Jobs/RestartStuckJobs",
            new()
        );
    }

    public async Task GoAsync()
    {
        try
        {
            (Error? error, StartDueJobsReply? startReply) = await _serviceClient.PostAsync<StartDueJobsRequest, StartDueJobsReply>(
                "Jobs/StartDueJobs",
                new()
            );

            if (error is not null)
            {
                throw new ApplicationException(error.Message);
            }

            // TODO: I don't remember what the purpose of this is
            if (startReply!.NumRunningJobs >= 10)
            {
                return;
            }

            (error, DequeueScheduledJobsReply? dequeueReply) = await _serviceClient.PostAsync<DequeueScheduledJobsRequest, DequeueScheduledJobsReply>(
                "Jobs/DequeueScheduledJobs",
                new()
            );

            if (error is not null)
            {
                throw new ApplicationException(error.Message);
            }

            foreach (var job in dequeueReply!.Jobs)
            {
                try
                {
                    _logger.LogInformation("Processing job {jobID}", job.ID);

                    var worker = new RunnerWorker(_serviceProvider, _config, job.Worker);

                    _logger.LogInformation("Executing job {JobID}", job.ID);
                    _logger.LogInformation("Worker={worker}", job.Worker.WorkerName);

                    worker.RunAsync().ContinueWith(async t =>
                    {
                        if (t.IsFaulted)
                        {
                            _logger.LogInformation("IsFaulted");
                            await CompleteWorkerAsync(job, new(Success: false,
                                t.Exception!.ToString())).ConfigureAwait(false);
                        }
                        else if (t.IsCanceled)
                        {
                            _logger.LogInformation("IsCanceled");
                            await CompleteWorkerAsync(job, new(Success: false,
                                "Cancelled for an unknown reason")).ConfigureAwait(false);
                        }
                        else if (t.IsCompleted)
                        {
                            _logger.LogInformation("IsCompleted");
                            await CompleteWorkerAsync(job, t.Result).ConfigureAwait(false);
                        }
                        else
                        {
                            _logger.LogInformation("ELSE");
                        }
                    }, TaskScheduler.Default).DoNotAwait();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error executing job {jobID}", job.ID);
                    await CompleteWorkerAsync(
                        job,
                        new(Success: false, ex.ToString())
                    ).ConfigureAwait(false);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Error in GoAsync");
        }
    }

    private async Task CompleteWorkerAsync(JobWithWorker jobWithWorker, WorkerResult workerResult)
    {
        (Error? error, _) = await _serviceClient.PostAsync<CompleteJobRequest, CompleteJobReply>(
            "Jobs/CompleteJob",
            new(jobWithWorker.ID, workerResult.Success, workerResult.DetailedMessage)
        );

        if (error is not null)
        {
            throw new ApplicationException(error.Message);
        }
    }
}
