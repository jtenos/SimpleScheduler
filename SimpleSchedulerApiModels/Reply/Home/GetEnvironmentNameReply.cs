using System.Runtime.Serialization;

namespace SimpleSchedulerApiModels.Reply.Home;

[DataContract]
public class GetEnvironmentNameReply
{
    public GetEnvironmentNameReply() { }

    public GetEnvironmentNameReply(string environmentName)
    {
        EnvironmentName = environmentName;
    }

    [DataMember(Order = 1)] public string EnvironmentName { get; set; } = default!;
}
