using Humanizer;

namespace SimpleSchedulerModels;

public record class Job(long ID, long ScheduleID, DateTime InsertDateUTC, DateTime QueueDateUTC,
    DateTime? CompleteDateUTC, string StatusCode, string AcknowledgementCode,
    DateTime? AcknowledgementDate, bool HasDetailedMessage)
{
    public JobStatus JobStatus { get; } = JobStatus.Parse(StatusCode);

    public string? FriendlyDuration
    {
        get
        {
            double? durationInSeconds = null;
            if (CompleteDateUTC.HasValue)
            {
                DateTime firstDate = InsertDateUTC;
                if (QueueDateUTC > InsertDateUTC)
                {
                    firstDate = QueueDateUTC;
                }
                durationInSeconds = CompleteDateUTC.Value.Subtract(firstDate).TotalSeconds;
            }
            if (!durationInSeconds.HasValue)
            {
                return null;
            }

            return TimeSpan.FromSeconds(durationInSeconds.Value).Humanize(precision: 1,
                maxUnit: Humanizer.Localisation.TimeUnit.Minute,
                minUnit: Humanizer.Localisation.TimeUnit.Second);
        }
    }
}
