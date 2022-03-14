using SimpleSchedulerService;
using SimpleSchedulerServiceClient;

Serilog.Debugging.SelfLog.Enable(msg => System.Diagnostics.Debug.WriteLine(msg));

await Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddSingleton(sp => new ServiceClient(
            new HttpClient { BaseAddress = new Uri(sp.GetRequiredService<IConfiguration>()["ApiUrl"]) }
        ));

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