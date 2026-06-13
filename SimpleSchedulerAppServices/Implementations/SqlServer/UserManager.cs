using Dapper;
using Microsoft.Extensions.Logging;
using SimpleSchedulerData;
using SimpleSchedulerEmail;

namespace SimpleSchedulerAppServices.Implementations.SqlServer;

/// <summary>
/// SQL Server user manager. Data access is via stored procedures in the [app] schema.
/// </summary>
public sealed class UserManager : UserManagerBase
{
    public UserManager(IDatabase db, IEmailer emailer, ILogger<UserManager> logger)
        : base(db, emailer, logger)
    {
    }

    protected override async Task<string[]> GetAllUserEmailsCoreAsync()
        => await Db.GetManyAsync<string>("[app].[Users_SelectAll]", parameters: null).ConfigureAwait(false);

    protected override async Task<LoginSubmitResult> SubmitLoginCoreAsync(string emailAddress)
    {
        DynamicParameters param = new DynamicParameters()
            .AddNVarCharParam("@EmailAddress", emailAddress, 200);

        return await Db.GetOneAsync<LoginSubmitResult>("[app].[Users_SubmitLogin]", param).ConfigureAwait(false);
    }

    protected override async Task<LoginValidateResult> ValidateLoginCoreAsync(Guid validationCode)
    {
        DynamicParameters param = new DynamicParameters()
            .AddUniqueIdentifierParam("@ValidationCode", validationCode);

        return await Db.GetOneAsync<LoginValidateResult>("[app].[Users_ValidateLogin]", param).ConfigureAwait(false);
    }
}
