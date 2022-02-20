using Serilog;
using SimpleSchedulerServiceChecker;

await Host.CreateDefaultBuilder()
    .ConfigureServices((hostContext, services) =>
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(hostContext.Configuration)
            .CreateLogger();

        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog();
        });

        services.AddHostedService<Worker>();
    })
    .UseWindowsService()
    .Build()
    .RunAsync();

