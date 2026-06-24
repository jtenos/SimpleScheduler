using Microsoft.Extensions.Logging;
using SimpleSchedulerAppServices.Interfaces;
using SimpleSchedulerData;
using SimpleSchedulerEmail;

namespace SimpleSchedulerAppServices.Implementations;

/// <summary>
/// Database-agnostic logic for the user manager (login email, internal auth short-circuit, result
/// handling). The provider-specific data access lives in the abstract Core methods.
/// </summary>
public abstract class UserManagerBase : IUserManager
{
    protected IDatabase Db { get; }
    private readonly IEmailer _emailer;
    private readonly ILogger _logger;

    protected UserManagerBase(IDatabase db, IEmailer emailer, ILogger logger)
    {
        Db = db;
        _emailer = emailer;
        _logger = logger;
    }

    // Classes with settable properties (not positional records) so Dapper can map SQLite's
    // INTEGER 0/1 result columns to bool (constructor injection doesn't allow that conversion).
    protected sealed class LoginSubmitResult
    {
        public bool Success { get; set; }
        public Guid ValidationCode { get; set; }
    }
    protected sealed class LoginValidateResult
    {
        public bool Success { get; set; }
        public string? EmailAddress { get; set; }
        public bool NotFound { get; set; }
        public bool Expired { get; set; }
    }

    // ---- provider-specific data access ----
    protected abstract Task<string[]> GetAllUserEmailsCoreAsync();
    protected abstract Task<LoginSubmitResult> SubmitLoginCoreAsync(string emailAddress);
    protected abstract Task<LoginValidateResult> ValidateLoginCoreAsync(Guid validationCode);

    // ---- agnostic orchestration ----
    async Task<string[]> IUserManager.GetAllUserEmailsAsync(bool allowLoginDropdown)
    {
        if (!allowLoginDropdown)
        {
            return Array.Empty<string>();
        }

        return await GetAllUserEmailsCoreAsync().ConfigureAwait(false);
    }

    async Task<bool> IUserManager.LoginSubmitAsync(string emailAddress, string webUrl)
    {
        _logger.LogInformation("LoginSubmitAsync({emailAddress}, {webUrl}", emailAddress, webUrl);

        LoginSubmitResult result = await SubmitLoginCoreAsync(emailAddress).ConfigureAwait(false);

        _logger.LogInformation("Result: {result}", result);

        if (!result.Success) { return false; }

        string url = $"{webUrl}/validate-user/{result.ValidationCode}";
        string body = $@"
            <a href='{url}' target=_blank>Click here to log in</a>
            <br><br>
            Or copy and paste the following:
            <br><br>
            {result.ValidationCode:D}
        ";
        await _emailer.SendEmailAsync(new[] { emailAddress }.ToArray(), $"Log In", body);

        return true;
    }

    async Task<string> IUserManager.LoginValidateAsync(Guid validationCode, Guid internalSecretAuthKey)
    {
        if (validationCode == internalSecretAuthKey)
        {
            return "@@internal@@";
        }

        LoginValidateResult result = await ValidateLoginCoreAsync(validationCode).ConfigureAwait(false);

        if (result.NotFound) { throw new ApplicationException("Not found"); }
        if (result.Expired) { throw new ApplicationException("Expired"); }
        if (!result.Success || result.EmailAddress is null) { throw new ApplicationException("Invalid call to LoginValidate"); }
        return result.EmailAddress;
    }
}
