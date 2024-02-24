namespace SimpleSchedulerBlazorWasm;

public class ClientAppInfo
{
    public string EnvironmentName { get; }
    public readonly Guid BuildGuid = Guid.NewGuid();

    public ClientAppInfo(IConfiguration config)
    {
        EnvironmentName = config["EnvironmentName"]!;
    }
}
