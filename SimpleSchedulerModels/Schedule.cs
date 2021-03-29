using System;

namespace SimpleSchedulerModels
{
    public record Schedule
    {
        public Schedule(int ScheduleID, bool IsActive, int WorkerID, bool Sunday, bool Monday,
            bool Tuesday, bool Wednesday, bool Thursday, bool Friday, bool Saturday,
            TimeSpan? TimeOfDayUTC, TimeSpan? RecurTime, TimeSpan? RecurBetweenStartUTC,
            TimeSpan? RecurBetweenEndUTC, bool OneTime)
            => (this.ScheduleID, this.IsActive, this.WorkerID, this.Sunday, this.Monday,
            this.Tuesday, this.Wednesday, this.Thursday, this.Friday, this.Saturday,
            this.TimeOfDayUTC, this.RecurTime, this.RecurBetweenStartUTC,
            this.RecurBetweenEndUTC, this.OneTime) = (ScheduleID, IsActive, WorkerID, Sunday,
            Monday, Tuesday, Wednesday, Thursday, Friday, Saturday, TimeOfDayUTC, RecurTime,
            RecurBetweenStartUTC, RecurBetweenEndUTC, OneTime);

        public Schedule(long ScheduleID, string UpdateDateTime, long IsActive,
            long WorkerID, long Sunday, long Monday,
            long Tuesday, long Wednesday, long Thursday, long Friday, long Saturday,
            string? TimeOfDayUTC, string? RecurTime, string? RecurBetweenStartUTC,
            string? RecurBetweenEndUTC, string OneTime)
            : this((int)ScheduleID, IsActive == 1, (int)WorkerID, Sunday == 1, 
                Monday == 1, Tuesday == 1, Wednesday == 1,
                Thursday == 1, Friday == 1, Saturday == 1,
                string.IsNullOrWhiteSpace(TimeOfDayUTC) ? (TimeSpan?)null : TimeSpan.Parse(TimeOfDayUTC),
                string.IsNullOrWhiteSpace(RecurTime) ? (TimeSpan?)null : TimeSpan.Parse(RecurTime),
                string.IsNullOrWhiteSpace(RecurBetweenStartUTC) ? (TimeSpan?)null: TimeSpan.Parse(RecurBetweenStartUTC),
                string.IsNullOrWhiteSpace(RecurBetweenEndUTC) ? (TimeSpan?)null: TimeSpan.Parse(RecurBetweenEndUTC),
                OneTime == "1"){}

        public int ScheduleID { get; }
        public bool IsActive { get; }
        public int WorkerID { get; }
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
