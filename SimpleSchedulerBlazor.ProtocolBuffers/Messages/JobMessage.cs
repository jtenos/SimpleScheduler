using Google.Protobuf.WellKnownTypes;
using SimpleSchedulerBlazor.ProtocolBuffers.Types;
using SimpleSchedulerModels;

namespace SimpleSchedulerBlazor.ProtocolBuffers.Messages;

partial class JobMessage
{
    public JobMessage(Job job)
    {
        ID = job.ID;
        ScheduleID = job.ScheduleID;
        InsertDateUTC = Timestamp.FromDateTime(job.InsertDateUTC);
        QueueDateUTC = Timestamp.FromDateTime(job.QueueDateUTC);
        CompleteDateUTC = new NullableTimestamp(job.CompleteDateUTC);
        StatusCode = job.StatusCode;
        AcknowledgementCode = job.AcknowledgementCode;
        AcknowledgementDate = new NullableTimestamp(job.AcknowledgementDate);
        HasDetailedMessage = job.HasDetailedMessage;
    }
}
