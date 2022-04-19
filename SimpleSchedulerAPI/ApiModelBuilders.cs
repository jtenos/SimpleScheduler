using SimpleSchedulerApiModels;

namespace SimpleSchedulerAPI;

internal static class ApiModelBuilders
{
    public static Job GetJob(SimpleSchedulerDomainModels.Job j)
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

    public static JobWithWorkerID GetJobWithWorkerID(SimpleSchedulerDomainModels.JobWithWorkerID j)
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

    public static JobWithWorker GetJobWithWorker(SimpleSchedulerDomainModels.JobWithWorker j)
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
            Worker: GetWorker(j.Worker)
        );
    }

    public static Schedule GetSchedule(SimpleSchedulerDomainModels.Schedule s)
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

    public static Worker GetWorker(SimpleSchedulerDomainModels.Worker w)
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
