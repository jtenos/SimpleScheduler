using Dapper;
using Microsoft.Extensions.Logging;
using SimpleSchedulerAppServices.Interfaces;
using SimpleSchedulerData;
using SimpleSchedulerEmail;

namespace SimpleSchedulerAppServices.Implementations;

public sealed class UserManager
    : IUserManager
{
    private readonly SqlDatabase _db;
    private readonly IEmailer _emailer;
    private readonly ILogger<UserManager> _logger;

    public UserManager(SqlDatabase db, IEmailer emailer, ILogger<UserManager> logger)
    {
        _db = db;
        _emailer = emailer;
        _logger = logger;
    }

    async Task<string[]> IUserManager.GetAllUserEmailsAsync(bool allowLoginDropdown)
    {
        if (!allowLoginDropdown)
        {
            return Array.Empty<string>();
        }

        return await _db.GetManyAsync<string>(
            "[app].[Users_SelectAll]",
            parameters: null
        ).ConfigureAwait(false);
    }

    private record class LoginSubmitResult(bool Success, Guid ValidationCode);
    async Task<bool> IUserManager.LoginSubmitAsync(string emailAddress, string webUrl)
    {
        _logger.LogInformation("LoginSubmitAsync({emailAddress}, {webUrl}",
            emailAddress, webUrl);

        DynamicParameters param = new DynamicParameters()
            .AddNVarCharParam("@EmailAddress", emailAddress, 200);

        LoginSubmitResult result = await _db.GetOneAsync<LoginSubmitResult>(
            "[app].[Users_SubmitLogin]",
            param
        ).ConfigureAwait(false);

        _logger.LogInformation("Result: {result}", result);

        if (!result.Success) { return false; }

        string url = $"{webUrl}/validate-user/{result.ValidationCode}";
        await _emailer.SendEmailAsync(new[] { emailAddress }.ToArray(),
            $"Log In",
            $"<a href='{url}' target=_blank>Click here to log in</a>");

        return true;
    }

    private record class LoginValidateResult(
        bool Success, string? EmailAddress, bool NotFound, bool Expired
    );
    async Task<string> IUserManager.LoginValidateAsync(Guid validationCode, Guid internalSecretAuthKey)
    {
        _logger.LogDebug("internalSecretAuthKey: {authKey}", internalSecretAuthKey);
        _logger.LogDebug("validationCode: {validationCode}", validationCode);

        if (validationCode == internalSecretAuthKey)
        {
            return "@@internal@@";
        }

        DynamicParameters param = new DynamicParameters()
            .AddUniqueIdentifierParam("@ValidationCode", validationCode);

        LoginValidateResult result = await _db.GetOneAsync<LoginValidateResult>(
            "[app].[Users_ValidateLogin]",
            param
        ).ConfigureAwait(false);

        if (result.NotFound) { throw new ApplicationException("Not found"); }
        if (result.Expired) { throw new ApplicationException("Expired"); }
        if (!result.Success || result.EmailAddress is null) { throw new ApplicationException("Invalid call to LoginValidate"); }
        return result.EmailAddress;
    }
}
