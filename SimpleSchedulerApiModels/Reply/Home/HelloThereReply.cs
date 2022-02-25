using System.Runtime.Serialization;

namespace SimpleSchedulerApiModels.Reply.Home;

[DataContract]
public class HelloThereReply
{
    public HelloThereReply() { }

    public HelloThereReply(string message)
    {
        Message = message;
    }

    [DataMember(Order = 1)] public string Message { get; set; } = default!;
}
