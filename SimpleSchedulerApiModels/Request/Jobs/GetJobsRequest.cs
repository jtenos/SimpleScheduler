namespace SimpleSchedulerApiModels.Request.Jobs;

public class GetJobsRequest
{
    public GetJobsRequest() { }

    public GetJobsRequest(
        long? workerID,
        string? statusCode,
        int pageNumber,
        bool overdueOnly)
    {
        WorkerID = workerID;
        StatusCode = statusCode;
        PageNumber = pageNumber;
        OverdueOnly = overdueOnly;
    }

    public long? WorkerID { get; set; }
    public string? StatusCode { get; set; }
    public int PageNumber { get; set; }
    public bool OverdueOnly { get; set; }
}
