using Serilog;
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

        services.AddSingleton<IEmailer>((sp) =>
        {
            IConfiguration config = sp.GetRequiredService<IConfiguration>();
            MailConfigSection mailConfig = config.GetSection("MailSettings").Get<MailConfigSection>();
            Emailer emailer = new(mailConfig, config["EnvironmentName"]);
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
