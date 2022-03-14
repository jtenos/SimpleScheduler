using SimpleSchedulerApiModels.Reply.Home;
using SimpleSchedulerApiModels.Request.Home;

namespace SimpleSchedulerBlazor.Server.ApiServices;

public static class HomeServiceMap
{
    private static Task<GetUtcNowReply> GetUtcNowAsync(GetUtcNowRequest request)
    {
        return Task.FromResult(new GetUtcNowReply(
            FormattedDateTime: $"{DateTime.UtcNow:MMM dd HH\\:mm} (UTC)"
        ));
    }

    private static Task<HelloThereReply> HelloThereAsync(HelloThereRequest request)
    {
        return Task.FromResult(new HelloThereReply(
            Message: "Howdy"
        ));
    }

    public static void MapHomeService(this WebApplication app)
    {
        app.MapPost("/Home/GetUtcNow", GetUtcNowAsync);
        app.MapPost("/Home/HelloThere", HelloThereAsync);
    }
}
