using Dapper;
using SimpleSchedulerAppServices.Interfaces;
using SimpleSchedulerConfiguration.Models;
using SimpleSchedulerData;
using SimpleSchedulerEmail;

namespace SimpleSchedulerAppServices.Implementations;

public sealed class UserManager
    : IUserManager
{
    private readonly SqlDatabase _db;
    private readonly AppSettings _appSettings;
    private readonly IEmailer _emailer;

    public UserManager(SqlDatabase db, AppSettings appSettings, IEmailer emailer)
    {
        _db = db;
        _appSettings = appSettings;
        _emailer = emailer;
    }

    async Task<string[]> IUserManager.GetAllUserEmailsAsync()
    {
        if (!_appSettings.AllowLoginDropDown)
        {
            return Array.Empty<string>();
        }

        return await _db.GetManyAsync<string>(
            "[app].[Users_SelectAll]",
            parameters: null
        ).ConfigureAwait(false);
    }

    private record class LoginSubmitResult(bool Success, Guid ValidationCode);
    async Task<bool> IUserManager.LoginSubmitAsync(string emailAddress)
    {
        DynamicParameters param = new DynamicParameters()
            .AddNVarCharParam("@EmailAddress", emailAddress, 200);

        LoginSubmitResult result = await _db.GetOneAsync<LoginSubmitResult>(
            "[app].[Users_SubmitLogin]",
            param
        ).ConfigureAwait(false);

        if (!result.Success) { return false; }

        string url = $"{_appSettings.WebUrl}/validate-user/{result.ValidationCode}";
        await _emailer.SendEmailAsync(new[] { emailAddress }.ToArray(),
            $"Scheduler ({_appSettings.EnvironmentName}) Log In",
            $"<a href='{url}' target=_blank>Click here to log in</a>");

        return true;
    }

    private record class LoginValidateResult(
        bool Success, string? EmailAddress, bool NotFound, bool Expired
    );
    async Task<string> IUserManager.LoginValidateAsync(Guid validationCode)
    {
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
