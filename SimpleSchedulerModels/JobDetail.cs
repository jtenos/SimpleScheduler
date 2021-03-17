namespace SimpleSchedulerModels
{
    public record JobDetail(Job Job, Schedule Schedule, Worker Worker);
}
