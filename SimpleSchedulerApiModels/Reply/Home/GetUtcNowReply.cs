using System.Text.Json.Serialization;

namespace SimpleSchedulerApiModels.Reply.Home;

public class GetUtcNowReply
{
    public GetUtcNowReply() { }

    public GetUtcNowReply(string formattedDateTime)
    {
        FormattedDateTime = formattedDateTime;
    }

    [JsonPropertyName("fmtDt")] public string FormattedDateTime { get; set; } = default!;
}
