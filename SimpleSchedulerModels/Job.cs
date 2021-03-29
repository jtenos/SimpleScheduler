using System;

namespace SimpleSchedulerModels
{
    public record Job
    {
        public Job(int JobID, int ScheduleID, DateTime InsertDateUTC, DateTime QueueDateUTC,
            DateTime? CompleteDateUTC, string StatusCode, string? DetailedMessage,
            Guid AcknowledgementID, DateTime? AcknowledgementDate)
            => (this.JobID, this.ScheduleID, this.InsertDateUTC, this.QueueDateUTC,
            this.CompleteDateUTC, this.StatusCode, this.DetailedMessage,
            this.AcknowledgementID, this.AcknowledgementDate) = (JobID, ScheduleID,
            InsertDateUTC, QueueDateUTC, CompleteDateUTC, StatusCode, DetailedMessage,
            AcknowledgementID, AcknowledgementDate);

        public Job(int JobID, DateTime UpdateDateTime, int ScheduleID, DateTime InsertDateUTC,
            DateTime QueueDateUTC, DateTime? CompleteDateUTC, string StatusCode,
            string? DetailedMessage, Guid AcknowledgementID, DateTime? AcknowledgementDate)
            : this(JobID, ScheduleID, InsertDateUTC, QueueDateUTC, CompleteDateUTC,
            StatusCode, DetailedMessage, AcknowledgementID, AcknowledgementDate){}

        public int JobID { get; }
        public int ScheduleID { get; }
        public DateTime InsertDateUTC { get; }
        public DateTime QueueDateUTC { get; }
        public DateTime? CompleteDateUTC { get; }
        public string StatusCode { get; }
        public string? DetailedMessage { get; }
        public Guid AcknowledgementID { get; }
        public DateTime? AcknowledgementDate { get; }
    }
}
