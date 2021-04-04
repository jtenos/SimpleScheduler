using System;

namespace SimpleSchedulerModels
{
    public record Schedule
    {
        public Schedule(long ScheduleID, bool IsActive, long WorkerID, bool Sunday, bool Monday,
            bool Tuesday, bool Wednesday, bool Thursday, bool Friday, bool Saturday,
            TimeSpan? TimeOfDayUTC, TimeSpan? RecurTime, TimeSpan? RecurBetweenStartUTC,
            TimeSpan? RecurBetweenEndUTC, bool OneTime)
            => (this.ScheduleID, this.IsActive, this.WorkerID, this.Sunday, this.Monday,
            this.Tuesday, this.Wednesday, this.Thursday, this.Friday, this.Saturday,
            this.TimeOfDayUTC, this.RecurTime, this.RecurBetweenStartUTC,
            this.RecurBetweenEndUTC, this.OneTime) = (ScheduleID, IsActive, WorkerID, Sunday,
            Monday, Tuesday, Wednesday, Thursday, Friday, Saturday, TimeOfDayUTC, RecurTime,
            RecurBetweenStartUTC, RecurBetweenEndUTC, OneTime);

        public long ScheduleID { get; }
        public bool IsActive { get; }
        public long WorkerID { get; }
        public bool Sunday { get; }
        public bool Monday { get; }
        public bool Tuesday { get; }
        public bool Wednesday { get; }
        public bool Thursday { get; }
        public bool Friday { get; }
        public bool Saturday { get; }
        public TimeSpan? TimeOfDayUTC { get; }
        public TimeSpan? RecurTime { get; }
        public TimeSpan? RecurBetweenStartUTC { get; }
        public TimeSpan? RecurBetweenEndUTC { get; }
        public bool OneTime { get; }
    }
}
