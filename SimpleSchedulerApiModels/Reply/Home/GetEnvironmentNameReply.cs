namespace SimpleSchedulerApiModels.Reply.Home;

public class GetEnvironmentNameReply
{
    public GetEnvironmentNameReply() { }

    public GetEnvironmentNameReply(string environmentName)
    {
        EnvironmentName = environmentName;
    }

    public string EnvironmentName { get; set; } = default!;
}
