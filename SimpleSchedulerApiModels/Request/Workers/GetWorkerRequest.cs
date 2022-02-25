using System.Runtime.Serialization;

namespace SimpleSchedulerApiModels.Request.Workers;

[DataContract]
public class GetWorkerRequest
{
    public GetWorkerRequest() { }

    public GetWorkerRequest(long id)
    {
        ID = id;
    }

    [DataMember(Order = 1)] public long ID { get; set; }
}
