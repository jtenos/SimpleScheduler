using Humanizer;
using SimpleSchedulerDataEntities;
using SimpleSchedulerModels;

namespace SimpleSchedulerAppServices;

internal static class ModelBuilders
{
    public static Job GetJob(JobEntity j)
    {
        return new(
            ID: j.ID,
            ScheduleID: j.ScheduleID,
            InsertDateUTC: j.InsertDateUTC,
            QueueDateUTC: j.QueueDateUTC,
            CompleteDateUTC: j.CompleteDateUTC,
            StatusCode: j.StatusCode,
            AcknowledgementCode: j.AcknowledgementCode,
            AcknowledgementDate: j.AcknowledgementDate,
            HasDetailedMessage: j.HasDetailedMessage,
            FriendlyDuration: GetFriendlyDuration(j)
        );
    }

    public static Schedule GetSchedule(ScheduleEntity s)
    {
        return new(
            ID: s.ID,
            IsActive: s.IsActive,
            WorkerID: s.WorkerID,
            Sunday: s.Sunday,
            Monday: s.Monday,
            Tuesday: s.Tuesday,
            Wednesday: s.Wednesday,
            Thursday: s.Thursday,
            Friday: s.Friday,
            Saturday: s.Saturday,
            TimeOfDayUTC: s.TimeOfDayUTC,
            RecurTime: s.RecurTime,
            RecurBetweenStartUTC: s.RecurBetweenStartUTC,
            RecurBetweenEndUTC: s.RecurBetweenEndUTC,
            OneTime: s.OneTime
        );
    }

    public static Worker GetWorker(WorkerEntity w)
    {
        return new Worker(
            ID: w.ID,
            IsActive: w.IsActive,
            WorkerName: w.WorkerName,
            DetailedDescription: w.DetailedDescription,
            EmailOnSuccess: w.EmailOnSuccess,
            ParentWorkerID: w.ParentWorkerID,
            TimeoutMinutes: w.TimeoutMinutes,
            DirectoryName: w.DirectoryName,
            Executable: w.Executable,
            ArgumentValues: w.ArgumentValues
        );
    }

    private static string? GetFriendlyDuration(JobEntity job)
    {
        double? durationInSeconds = null;
        if (job.CompleteDateUTC.HasValue)
        {
            DateTime firstDate = job.InsertDateUTC;
            if (job.QueueDateUTC > job.InsertDateUTC)
            {
                firstDate = job.QueueDateUTC;
            }
            durationInSeconds = job.CompleteDateUTC.Value.Subtract(firstDate).TotalSeconds;
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
