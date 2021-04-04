namespace SimpleSchedulerEntities
{
    public record ScheduleEntity(
        long ScheduleID,
        long IsActive,
        long WorkerID,
        long Sunday,
        long Monday,
        long Tuesday,
        long Wednesday,
        long Thursday,
        long Friday,
        long Saturday,
        long? TimeOfDayUTC,
        long? RecurTime,
        long? RecurBetweenStartUTC,
        long? RecurBetweenEndUTC,
        long OneTime
    );
}
