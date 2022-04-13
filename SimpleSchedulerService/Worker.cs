using SimpleSchedulerApiModels.Reply.Login;
using SimpleSchedulerApiModels.Request.Login;
using SimpleSchedulerServiceClient;
using Timer = System.Timers.Timer;

namespace SimpleSchedulerService;

public class Worker
    : BackgroundService
{
    private readonly JobScheduler _scheduler;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly Guid _internalSecretAuthKey;
    private readonly ITokenLookup _tokenLookup;
    private static readonly Timer _authTimer = new(TimeSpan.FromMinutes(30).TotalMilliseconds);

    public Worker(JobScheduler scheduler, ITokenLookup tokenLookup,
        IServiceScopeFactory serviceScopeFactory, IConfiguration config)
    {
        _scheduler = scheduler;
        _tokenLookup = tokenLookup;
        _serviceScopeFactory = serviceScopeFactory;
        _internalSecretAuthKey = config.GetValue<Guid>("InternalSecretAuthKey");
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.CompletedTask;

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await RefreshAuthTokenAsync();
        _authTimer.Elapsed += async (sender, e) => await RefreshAuthTokenAsync();
        _authTimer.Start();

        await _scheduler.StartSchedulerAsync();
    }

    private async Task RefreshAuthTokenAsync()
    {
        using IServiceScope scope = _serviceScopeFactory.CreateAsyncScope();
        ServiceClient client = scope.ServiceProvider.GetRequiredService<ServiceClient>();
        (Error? error, ValidateEmailReply? reply) = await client.PostAsync<ValidateEmailRequest, ValidateEmailReply>(
            "Login/ValidateEmail",
            new(_internalSecretAuthKey)
        );

        if (error is not null)
        {
            throw new ApplicationException($"Error authenticating for service. Make sure InternalSecretAuthKey in the config matches the value in the API config: {error.Message}");
        }

        ((TokenLookup)_tokenLookup).Token = reply!.JwtToken;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _scheduler.Dispose();
        return Task.CompletedTask;
    }
}
