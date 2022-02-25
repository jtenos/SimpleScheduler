using System.Runtime.Serialization;

namespace SimpleSchedulerApiModels.Request.Jobs;

[DataContract]
public class GetJobsRequest
{
    public GetJobsRequest() { }

    public GetJobsRequest(
        long? workerID,
        string statusCode,
        int pageNumber,
        bool overdueOnly)
    {
        WorkerID = workerID;
        StatusCode = statusCode;
        PageNumber = pageNumber;
        OverdueOnly = overdueOnly;
    }

    [DataMember(Order = 1)] public long? WorkerID { get; set; }
    [DataMember(Order = 2)] public string StatusCode { get; set; } = default!;
    [DataMember(Order = 3)] public int PageNumber { get; set; }
    [DataMember(Order = 4)] public bool OverdueOnly { get; set; }
}
