using SimpleSchedulerApiModels.Reply.Login;
using SimpleSchedulerApiModels.Request.Login;
using SimpleSchedulerServiceClient;

namespace SimpleSchedulerServiceChecker;
internal class TokenLookup
    : ITokenLookup
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<TokenLookup> _logger;
    private readonly IConfiguration _config;

    public TokenLookup(IServiceScopeFactory serviceScopeFactory, 
        ILogger<TokenLookup> logger, IConfiguration config)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
        _config = config;
    }

    async Task<string?> ITokenLookup.LookupTokenAsync()
    {
        try
        {
            Guid internalSecretAuthKey = _config.GetValue<Guid>("InternalSecretAuthKey");
            //_logger.LogDebug("Internal secret auth key: {authKey}", internalSecretAuthKey);

            using IServiceScope scope = _serviceScopeFactory.CreateAsyncScope();
            ServiceClient client = scope.ServiceProvider.GetRequiredService<ServiceClient>();
            (Error? error, ValidateEmailReply? reply) = await client.PostAsync<ValidateEmailRequest, ValidateEmailReply>(
                "Login/ValidateEmail",
                new(internalSecretAuthKey),
                forceUnauthenticated: true
            );

            if (error is not null)
            {
                throw new ApplicationException($"Error authenticating for service. Make sure InternalSecretAuthKey in the config matches the value in the API config: {error.Message}");
            }

            if (reply is null)
            {
                throw new ApplicationException("Both error and reply are null when validating secret auth key");
            }

            _logger.LogInformation("reply: {reply}", reply);

            return reply!.JwtToken;
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Error setting token");
            throw;
        }
    }
}
