using SimpleSchedulerBlazor.ProtocolBuffers.Types;
using SimpleSchedulerModels;
using System.Collections.Immutable;

namespace SimpleSchedulerBlazor.ProtocolBuffers.Messages.Jobs;

partial class GetJobsRequest
{
    public GetJobsRequest(long? workerID, string? statusCode, int pageNumber, bool overdueOnly)
    {
        WorkerID = new NullableInt64(workerID);
        StatusCode = statusCode;
        PageNumber = pageNumber;
        OverdueOnly = overdueOnly;
    }
}
partial class GetJobsReply
{
    public GetJobsReply(ImmutableArray<Job> jobs)
    {
        foreach (Job job in jobs)
        {
            Jobs.Add(new JobMessage(job));
        }
    }
}
