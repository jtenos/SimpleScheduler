using SimpleSchedulerApiModels.Reply.Home;
using SimpleSchedulerApiModels.Request.Home;

namespace SimpleSchedulerBlazor.Server.ApiServices;

public static class HomeServiceMap
{
    public static void MapHomeService(this WebApplication app)
    {
        app.MapPost("/Home/GetUtcNow",
            (
                GetUtcNowRequest request
            ) =>
            {
                return new GetUtcNowReply(
                    FormattedDateTime: DateTime.UtcNow.ToString("MMM dd yyyy HH\\:mm\\:ss")
                );
            });

        app.MapPost("/Home/HelloThere",
            (
                HelloThereRequest request
            ) =>
            {
                return new HelloThereReply (
                    Message: "Howdy"
                );
            });
    }
}
