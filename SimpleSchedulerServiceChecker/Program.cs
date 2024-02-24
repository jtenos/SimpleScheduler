using Serilog;
using SimpleSchedulerEmail;
using SimpleSchedulerSerilogEmail;
using SimpleSchedulerServiceChecker;
using SimpleSchedulerServiceClient;

Serilog.Debugging.SelfLog.Enable(msg => System.Diagnostics.Debug.WriteLine(msg));

await Host.CreateDefaultBuilder()
    .ConfigureAppConfiguration((hostContext, builder) =>
    {
        builder.AddUserSecrets<Program>();
    })
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

            IEmailer emailer;
            if (!string.IsNullOrWhiteSpace(config["EmailFolder"]))
            {
                emailer = new LogFileEmailer(config["EmailFolder"]!);
            }
            else
            {
                var mailSettings = config.MailSettings();

                emailer = new Emailer(
                    Port: mailSettings.Port,
                    EmailFrom: mailSettings.EmailFrom,
                    AdminEmail: mailSettings.AdminEmail,
                    Host: mailSettings.Host,
                    UserName: mailSettings.UserName,
                    Password: mailSettings.Password,
                    EnvironmentName: config.EnvironmentName()
                );
            }

            EmailSink.SetEmailer(emailer);
            return emailer;
        });

        services.AddSingleton(sp => new ServiceClient(
            httpClient: new HttpClient { BaseAddress = new Uri(sp.GetRequiredService<IConfiguration>()["ApiUrl"]!) },
            tokenLookup: sp.GetRequiredService<ITokenLookup>(),
            () => { },
            sp.GetRequiredService<ILogger<ServiceClient>>()
        ));

        services.AddSingleton<ITokenLookup, TokenLookup>();
        services.AddHostedService<Worker>();
    })
    .UseWindowsService()
    .Build()
    .RunAsync();
