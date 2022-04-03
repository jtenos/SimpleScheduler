using Serilog;
using SimpleSchedulerService;
using SimpleSchedulerServiceClient;

Serilog.Debugging.SelfLog.Enable(msg => System.Diagnostics.Debug.WriteLine(msg));

await Host.CreateDefaultBuilder(args)
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
            new HttpClient { BaseAddress = new Uri(sp.GetRequiredService<IConfiguration>()["ApiUrl"]) },
            sp.GetRequiredService<JwtContainer>(),
            () => throw new ApplicationException("Unauthorized"),
            sp.GetRequiredService<ILogger<ServiceClient>>()
        ));

        services.AddSingleton<JwtContainer>();
        services.AddSingleton<JobExecutor>();
        services.AddSingleton<JobScheduler>();
        services.AddHostedService<Worker>();
    })
    .ConfigureHostConfiguration(configure =>
    {
        configure.AddJsonFile("secrets.json", optional: true);
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