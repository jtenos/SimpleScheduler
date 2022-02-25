using System.Runtime.Serialization;

namespace SimpleSchedulerApiModels.Request.Jobs;

[DataContract]
public class AcknowledgeErrorRequest
{
    public AcknowledgeErrorRequest() { }

    public AcknowledgeErrorRequest(long id)
    {
        ID = id;
    }

    [DataMember(Order = 1)] public long ID { get; set; }
}
