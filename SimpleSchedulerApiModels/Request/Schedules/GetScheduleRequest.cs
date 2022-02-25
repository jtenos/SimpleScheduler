using System.Runtime.Serialization;

namespace SimpleSchedulerApiModels.Request.Schedules;

[DataContract]
public class GetScheduleRequest
{
    public GetScheduleRequest() { }

    public GetScheduleRequest(long id)
    {
        ID = id;
    }

    [DataMember(Order = 1)] public long ID { get; set; }
}
