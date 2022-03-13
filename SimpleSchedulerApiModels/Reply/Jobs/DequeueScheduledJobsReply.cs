namespace SimpleSchedulerApiModels.Reply.Jobs;

public record class DequeueScheduledJobsReply(JobWithWorker[] Jobs);
