using Serilog;
using SimpleSchedulerService;
using SimpleSchedulerServiceClient;

Serilog.Debugging.SelfLog.Enable(msg => System.Diagnostics.Debug.WriteLine(msg));

await Host.CreateDefaultBuilder(args)
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

        services.AddScoped(sp => new ServiceClient(
            httpClient: new HttpClient { BaseAddress = new Uri(sp.GetRequiredService<IConfiguration>()["ApiUrl"]!) },
            tokenLookup: sp.GetRequiredService<ITokenLookup>(),
            redirectToLogin: () => throw new ApplicationException("Unauthorized"),
            logger: sp.GetRequiredService<ILogger<ServiceClient>>()
        ));

        services.AddSingleton<ITokenLookup, TokenLookup>();
        services.AddSingleton<JobExecutor>();
        services.AddSingleton<JobScheduler>();
        services.AddHostedService<Worker>();
    })
    .UseWindowsService()
    .Build()
    .RunAsync();
/* 
INSTALLATION:

POWERSHELL:

(all on one line):
New-Service -Name Scheduler -BinaryPathName
c:\services\SimpleScheduler\SimpleSchedulerService.exe -DisplayName "Scheduler"
-Description "Scheduler" -StartupType Automatic
-Credential MyDomain\MyUser

net start Scheduler
*/