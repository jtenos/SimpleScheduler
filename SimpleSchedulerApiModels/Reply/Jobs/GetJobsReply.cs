namespace SimpleSchedulerApiModels.Reply.Jobs;

public record class GetJobsReply(
    JobWithWorkerID[] Jobs
);
