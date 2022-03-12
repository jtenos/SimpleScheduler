using SimpleSchedulerApiModels;

namespace SimpleSchedulerBlazor.Server;

internal static class ApiModelBuilders
{
    public static Job GetJob(SimpleSchedulerModels.Job j)
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
            FriendlyDuration: j.FriendlyDuration
        );
    }

    public static JobWithWorkerID GetJobWithWorkerID(SimpleSchedulerModels.JobWithWorkerID j)
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
            FriendlyDuration: j.FriendlyDuration,
            WorkerID: j.WorkerID
        );
    }

    public static Schedule GetSchedule(SimpleSchedulerModels.Schedule s)
    {
        return new(
            id: s.ID,
            isActive: s.IsActive,
            workerID: s.WorkerID,
            sunday: s.Sunday,
            monday: s.Monday,
            tuesday: s.Tuesday,
            wednesday: s.Wednesday,
            thursday: s.Thursday,
            friday: s.Friday,
            saturday: s.Saturday,
            timeOfDayUTC: s.TimeOfDayUTC,
            recurTime: s.RecurTime,
            recurBetweenStartUTC: s.RecurBetweenStartUTC,
            recurBetweenEndUTC: s.RecurBetweenEndUTC,
            oneTime: s.OneTime
        );
    }

    public static Worker GetWorker(SimpleSchedulerModels.Worker w)
    {
        return new(
            id: w.ID,
            isActive: w.IsActive,
            workerName: w.WorkerName,
            detailedDescription: w.DetailedDescription,
            emailOnSuccess: w.EmailOnSuccess,
            parentWorkerID: w.ParentWorkerID,
            timeoutMinutes: w.TimeoutMinutes,
            directoryName: w.DirectoryName,
            executable: w.Executable,
            argumentValues: w.ArgumentValues
        );
    }
}
