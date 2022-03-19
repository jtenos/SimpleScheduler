using Serilog;
using SimpleSchedulerEmail;
using SimpleSchedulerSerilogEmail;
using SimpleSchedulerServiceChecker;
using SimpleSchedulerServiceClient;

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
            Emailer emailer = new(
                Port: config.GetValue<int>("MailSettings:Port"),
                EmailFrom: config["MailSettings:EmailFrom"],
                AdminEmail: config["MailSettings:AdminEmail"],
                Host: config["MailSettings:Host"],
                UserName: config["MailSettings:UserName"],
                Password: config["MailSettings:Password"],
                EnvironmentName: config["EnvironmentName"]);
            EmailSink.SetEmailer(emailer);
            return emailer;
        });

        services.AddSingleton(sp => new ServiceClient(
            new HttpClient { BaseAddress = new Uri(sp.GetRequiredService<IConfiguration>()["ApiUrl"]) },
            sp.GetRequiredService<JwtContainer>()
        ));

        services.AddSingleton<JwtContainer>();
        services.AddHostedService<Worker>();
    })
    .ConfigureHostConfiguration(configure =>
    {
        configure.AddJsonFile("secrets.json", optional: true);
    })
    .UseWindowsService()
    .Build()
    .RunAsync();
