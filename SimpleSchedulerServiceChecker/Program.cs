using Serilog;
using SimpleSchedulerConfiguration.Models;
using SimpleSchedulerEmail;
using SimpleSchedulerSerilogEmail;
using SimpleSchedulerServiceChecker;

Serilog.Debugging.SelfLog.Enable(msg => System.Diagnostics.Debug.WriteLine(msg));

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

        AppSettings appSettings = hostContext.Configuration.GetSection("AppSettings").Get<AppSettings>();
        services.AddSingleton(appSettings);

        services.AddSingleton<IEmailer>((sp) =>
        {
            Emailer emailer = new(sp.GetRequiredService<AppSettings>());
            EmailSink.SetEmailer(emailer);
            return emailer;
        });

        services.AddHostedService<Worker>();
    })
    .ConfigureHostConfiguration(configure =>
    {
        configure.AddJsonFile("secrets.json", optional: true);
    })
    .UseWindowsService()
    .Build()
    .RunAsync();
