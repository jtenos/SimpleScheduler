using System.Runtime.Serialization;

namespace SimpleSchedulerApiModels.Request.Jobs;

[DataContract]
public class CancelJobRequest
{
    public CancelJobRequest() { }

    public CancelJobRequest(long id)
    {
        ID = id;
    }

    [DataMember(Order = 1)] public long ID { get; set; }
}
