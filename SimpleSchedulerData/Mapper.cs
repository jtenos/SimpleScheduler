using System.Data.Common;
using SimpleSchedulerEntities;

namespace SimpleSchedulerData
{
    public static class Mapper
    {
        private static long GetInt64(DbDataReader rdr, string fieldName)
            => rdr.GetInt64(rdr.GetOrdinal(fieldName));
        private static long? GetInt64Nullable(DbDataReader rdr, string fieldName)
        {
            int idx = rdr.GetOrdinal(fieldName);
            return rdr.IsDBNull(idx) ? default : rdr.GetInt64(idx);
        }
        private static string GetString(DbDataReader rdr, string fieldName)
            => rdr.GetString(rdr.GetOrdinal(fieldName));
        private static string? GetStringNullable(DbDataReader rdr, string fieldName)
        {
            int idx = rdr.GetOrdinal(fieldName);
            return rdr.IsDBNull(idx) ? default : rdr.GetString(idx);
        }

        public static JobEntity MapJob(DbDataReader rdr)
            => new JobEntity(
                JobID: GetInt64(rdr, nameof(JobEntity.JobID)),
                ScheduleID: GetInt64(rdr, nameof(JobEntity.ScheduleID)),
                InsertDateUTC: GetInt64(rdr, nameof(JobEntity.InsertDateUTC)),
                QueueDateUTC: GetInt64(rdr, nameof(JobEntity.QueueDateUTC)),
                CompleteDateUTC: GetInt64Nullable(rdr, nameof(JobEntity.CompleteDateUTC)),
                StatusCode: GetString(rdr, nameof(JobEntity.StatusCode)),
                DetailedMessage: GetStringNullable(rdr, nameof(JobEntity.DetailedMessage)),
                AcknolwedgementID: GetString(rdr, nameof(JobEntity.AcknolwedgementID)),
                AcknowledgementDate: GetInt64Nullable(rdr, nameof(JobEntity.AcknowledgementDate))
            );

        public static LoginAttemptEntity MapLoginAttempt(DbDataReader rdr)
            => new LoginAttemptEntity(
                LoginAttemptID: GetInt64(rdr, nameof(LoginAttemptEntity.LoginAttemptID)),
                SubmitDateUTC: GetInt64(rdr, nameof(LoginAttemptEntity.SubmitDateUTC)),
                EmailAddress: GetString(rdr, nameof(LoginAttemptEntity.EmailAddress)),
                ValidationKey: GetString(rdr, nameof(LoginAttemptEntity.ValidationKey)),
                ValidationDateUTC: GetInt64(rdr, nameof(LoginAttemptEntity.ValidationDateUTC))
            );

        public static ScheduleEntity MapSchedule(DbDataReader rdr)
            => new ScheduleEntity(
                ScheduleID: GetInt64(rdr, nameof(ScheduleEntity.ScheduleID)),
                IsActive: GetInt64(rdr, nameof(ScheduleEntity.IsActive)),
                WorkerID: GetInt64(rdr, nameof(ScheduleEntity.WorkerID)),
                Sunday: GetInt64(rdr, nameof(ScheduleEntity.Sunday)),
                Monday: GetInt64(rdr, nameof(ScheduleEntity.Monday)),
                Tuesday: GetInt64(rdr, nameof(ScheduleEntity.Tuesday)),
                Wednesday: GetInt64(rdr, nameof(ScheduleEntity.Wednesday)),
                Thursday: GetInt64(rdr, nameof(ScheduleEntity.Thursday)),
                Friday: GetInt64(rdr, nameof(ScheduleEntity.Friday)),
                Saturday: GetInt64(rdr, nameof(ScheduleEntity.Saturday)),
                TimeOfDayUTC: GetInt64Nullable(rdr, nameof(ScheduleEntity.TimeOfDayUTC)),
                RecurTime: GetInt64Nullable(rdr, nameof(ScheduleEntity.RecurTime)),
                RecurBetweenStartUTC: GetInt64Nullable(rdr, nameof(ScheduleEntity.RecurBetweenStartUTC)),
                RecurBetweenEndUTC: GetInt64Nullable(rdr, nameof(ScheduleEntity.RecurBetweenEndUTC)),
                OneTime: GetInt64(rdr, nameof(ScheduleEntity.OneTime))
            );

        public static UserEntity MapUser(DbDataReader rdr)
            => new UserEntity(
                EmailAddress: GetString(rdr, nameof(UserEntity.EmailAddress))
            );

        public static WorkerEntity MapWorker(DbDataReader rdr)
            => new WorkerEntity(
                WorkerID: GetInt64(rdr, nameof(WorkerEntity.WorkerID)),
                IsActive: GetInt64(rdr, nameof(WorkerEntity.IsActive)),
                WorkerName: GetString(rdr, nameof(WorkerEntity.WorkerName)),
                DetailedDescription: GetString(rdr, nameof(WorkerEntity.DetailedDescription)),
                EmailOnSuccess: GetString(rdr, nameof(WorkerEntity.EmailOnSuccess)),
                ParentWorkerID: GetInt64Nullable(rdr, nameof(WorkerEntity.ParentWorkerID)),
                TimeoutMinutes: GetInt64(rdr, nameof(WorkerEntity.TimeoutMinutes)),
                DirectoryName: GetString(rdr, nameof(WorkerEntity.DirectoryName)),
                Executable: GetString(rdr, nameof(WorkerEntity.Executable)),
                ArgumentValues: GetString(rdr, nameof(WorkerEntity.ArgumentValues))
            );
    }
}
