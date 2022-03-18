using SimpleSchedulerApiModels.Reply.Login;
using SimpleSchedulerApiModels.Request.Login;
using SimpleSchedulerServiceClient;

namespace SimpleSchedulerService;

public class Worker
    : BackgroundService
{
    private readonly JobScheduler _scheduler;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly Guid _internalSecretAuthKey;

    public Worker(JobScheduler scheduler, IServiceScopeFactory serviceScopeFactory, IConfiguration config)
    {
        _scheduler = scheduler;
        _serviceScopeFactory = serviceScopeFactory;
        _internalSecretAuthKey = config.GetValue<Guid>("InternalSecretAuthKey");
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.CompletedTask;

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        using (IServiceScope scope = _serviceScopeFactory.CreateAsyncScope())
        {
            ServiceClient client = scope.ServiceProvider.GetRequiredService<ServiceClient>();
            (Error? error, ValidateEmailReply? reply) = await client.PostAsync<ValidateEmailRequest, ValidateEmailReply>(
                "Login/ValidateEmail",
                new(_internalSecretAuthKey)
            );

            if (error is not null)
            {
                throw new ApplicationException($"Error authenticating for service. Make sure InternalSecretAuthKey in the config matches the value in the API config: {error.Message}");
            }

            scope.ServiceProvider.GetRequiredService<JwtContainer>().Token = reply!.JwtToken;
        }

        await _scheduler.StartSchedulerAsync();
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _scheduler.Dispose();
        return Task.CompletedTask;
    }
}
