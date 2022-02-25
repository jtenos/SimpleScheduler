using System.Runtime.Serialization;

namespace SimpleSchedulerApiModels.Request.Schedules;

[DataContract]
public class ReactivateScheduleRequest
{
    public ReactivateScheduleRequest() { }

    public ReactivateScheduleRequest(long id)
    {
        ID = id;
    }

    [DataMember(Order = 1)] public long ID { get; set; }
}
