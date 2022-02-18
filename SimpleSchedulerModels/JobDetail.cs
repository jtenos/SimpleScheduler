namespace SimpleSchedulerModels;

public record class JobDetail(Job Job, Schedule Schedule, Worker Worker);
