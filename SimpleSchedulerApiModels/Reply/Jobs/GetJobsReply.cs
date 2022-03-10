namespace SimpleSchedulerApiModels.Reply.Jobs;

public class GetJobsReply
{
    public GetJobsReply() { }

    public GetJobsReply(JobWithWorkerID[] jobs)
    {
        Jobs = jobs;
    }

    public JobWithWorkerID[] Jobs { get; set; } = default!;
}
