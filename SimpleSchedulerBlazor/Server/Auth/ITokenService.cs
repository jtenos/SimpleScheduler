namespace SimpleSchedulerBlazor.Server.Auth;

public interface ITokenService
{
    string BuildToken(IConfiguration config, string emailAddress);
}
