namespace SimpleSchedulerApiModels.Reply.Jobs;

public class GetJobsReply
{
    public GetJobsReply() { }

    public GetJobsReply(Job[] jobs)
    {
        Jobs = jobs;
    }

    public Job[] Jobs { get; set; } = default!;
}
