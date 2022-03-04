namespace SimpleSchedulerApiModels.Reply.Home;

public class HelloThereReply
{
    public HelloThereReply() { }

    public HelloThereReply(string message)
    {
        Message = message;
    }

    public string Message { get; set; } = default!;
}
