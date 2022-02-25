using System.Runtime.Serialization;

namespace SimpleSchedulerApiModels.Request.Schedules;

[DataContract]
public class DeleteScheduleRequest
{
    public DeleteScheduleRequest() { }

    public DeleteScheduleRequest(long id)
    {
        ID = id;
    }

    [DataMember(Order = 1)] public long ID { get; set; }
}
