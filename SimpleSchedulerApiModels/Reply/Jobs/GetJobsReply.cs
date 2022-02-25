using System.Runtime.Serialization;

namespace SimpleSchedulerApiModels.Reply.Jobs;

[DataContract]
public class GetJobsReply
{
    public GetJobsReply() { }

    public GetJobsReply(Job[] jobs)
    {
        Jobs = jobs;
    }

    [DataMember(Order = 1)] public Job[] Jobs { get; set; } = default!;
}
