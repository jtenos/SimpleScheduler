using System.Runtime.Serialization;

namespace SimpleSchedulerApiModels.Request.Jobs;

[DataContract]
public class GetDetailedMessageRequest
{
    public GetDetailedMessageRequest() { }

    public GetDetailedMessageRequest(long id)
    {
        ID = id;
    }

    [DataMember(Order = 1)] public long ID { get; set; }
}
