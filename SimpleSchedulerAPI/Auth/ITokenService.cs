namespace SimpleSchedulerAPI.Auth;

public interface ITokenService
{
    string BuildToken(IConfiguration config, string emailAddress);
}
