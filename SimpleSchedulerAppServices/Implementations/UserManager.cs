using System.Collections.Immutable;
using Dapper;
using OneOf;
using OneOf.Types;
using SimpleSchedulerAppServices.Interfaces;
using SimpleSchedulerConfiguration.Models;
using SimpleSchedulerData;
using SimpleSchedulerEmail;
using SimpleSchedulerModels.ResultTypes;

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

    async Task<ImmutableArray<string>> IUserManager.GetAllUserEmailsAsync(CancellationToken cancellationToken)
    {
        if (!_appSettings.AllowLoginDropDown)
        {
            return ImmutableArray<string>.Empty;
        }

        return await _db.GetManyAsync<string>(
            "[app].[Users_SelectAll]",
            parameters: null,
            cancellationToken
        ).ConfigureAwait(false);
    }

    private record class LoginSubmitResult(bool Success, Guid ValidationCode);
    async Task<bool> IUserManager.LoginSubmitAsync(string emailAddress, CancellationToken cancellationToken)
    {
        DynamicParameters param = new DynamicParameters()
            .AddNVarCharParam("@EmailAddress", emailAddress, 200);

        LoginSubmitResult result = await _db.GetOneAsync<LoginSubmitResult>(
            "[app].[Users_SubmitLogin]",
            param,
            cancellationToken
        ).ConfigureAwait(false);

        if (!result.Success) { return false; }

        string url = $"{_appSettings.WebUrl}/validate-user/{result.ValidationCode}";
        await _emailer.SendEmailAsync(new[] { emailAddress }.ToImmutableArray(),
            $"Scheduler ({_appSettings.EnvironmentName}) Log In",
            $"<a href='{url}' target=_blank>Click here to log in</a>",
            cancellationToken);

        return true;
    }

    private record class LoginValidateResult(
        bool Success, string? EmailAddress, bool NotFound, bool Expired
    );
    async Task<OneOf<string, NotFound, Expired>> IUserManager.LoginValidateAsync(Guid validationCode,
        CancellationToken cancellationToken)
    {
        DynamicParameters param = new DynamicParameters()
            .AddUniqueIdentifierParam("@ValidationCode", validationCode);

        LoginValidateResult result = await _db.GetOneAsync<LoginValidateResult>(
            "[app].[Users_ValidateLogin]",
            param,
            cancellationToken
        ).ConfigureAwait(false);

        if (result.NotFound) { return new NotFound(); }
        if (result.Expired) { return new Expired(); }
        if (!result.Success || result.EmailAddress is null) { throw new ApplicationException("Invalid call to LoginValidate"); }
        return result.EmailAddress;
    }
}
