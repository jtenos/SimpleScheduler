using System.Runtime.Serialization;

namespace SimpleSchedulerApiModels.Request.Workers;

[DataContract]
public class ReactivateWorkerRequest
{
    public ReactivateWorkerRequest() { }

    public ReactivateWorkerRequest(long id)
    {
        ID = id;
    }

    [DataMember(Order = 1)] public long ID { get; set; }
}
