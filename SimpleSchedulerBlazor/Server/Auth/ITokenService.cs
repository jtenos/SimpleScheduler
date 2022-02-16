namespace SimpleSchedulerBlazor.Server.Auth;

public interface ITokenService
{
    string BuildToken(string key, string issuer, string emailAddress);
}
