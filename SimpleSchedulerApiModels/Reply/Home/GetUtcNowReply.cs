namespace SimpleSchedulerApiModels.Reply.Home;

public class GetUtcNowReply
{
    public GetUtcNowReply() { }

    public GetUtcNowReply(string formattedDateTime)
    {
        FormattedDateTime = formattedDateTime;
    }

    public string FormattedDateTime { get; set; } = default!;
}
