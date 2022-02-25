using System.Runtime.Serialization;

namespace SimpleSchedulerApiModels.Request.Workers;

[DataContract]
public class DeleteWorkerRequest
{
    public DeleteWorkerRequest()
    {
    }

    public DeleteWorkerRequest(long id)
    {
        ID = id;
    }

    [DataMember(Order = 1)] public long ID { get; set; }
}
