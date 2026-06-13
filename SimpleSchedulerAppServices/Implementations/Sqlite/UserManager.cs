using Dapper;
using Microsoft.Extensions.Logging;
using SimpleSchedulerData;
using SimpleSchedulerEmail;

namespace SimpleSchedulerAppServices.Implementations.Sqlite;

/// <summary>
/// SQLite user manager. Data access is via SQL scripts equivalent to the SQL Server procedures.
/// The login validation flow (branching result sets in T-SQL) is expressed as a few sequential
/// queries here.
/// </summary>
public sealed class UserManager : UserManagerBase
{
    public UserManager(IDatabase db, IEmailer emailer, ILogger<UserManager> logger)
        : base(db, emailer, logger)
    {
    }

    private sealed record LoginAttemptRow(long ID, DateTime SubmitDateUTC, string EmailAddress);

    protected override async Task<string[]> GetAllUserEmailsCoreAsync()
        => await Db.GetManyAsync<string>(
            "SELECT EmailAddress FROM Users ORDER BY EmailAddress;", parameters: null).ConfigureAwait(false);

    protected override async Task<LoginSubmitResult> SubmitLoginCoreAsync(string emailAddress)
    {
        // SQLite has no NEWID(); generate the validation code here and echo it back.
        Guid validationCode = Guid.NewGuid();

        DynamicParameters param = new();
        param.Add("@EmailAddress", emailAddress);
        param.Add("@ValidationCode", validationCode);

        return await Db.GetOneAsync<LoginSubmitResult>(@"
            INSERT INTO LoginAttempts (EmailAddress, ValidationCode)
            SELECT @EmailAddress, @ValidationCode
            WHERE EXISTS (SELECT 1 FROM Users WHERE EmailAddress = @EmailAddress);

            SELECT
                CASE WHEN changes() = 0 THEN 0 ELSE 1 END AS Success
                ,CASE WHEN changes() = 0 THEN '00000000-0000-0000-0000-000000000000' ELSE @ValidationCode END AS ValidationCode;",
            param
        ).ConfigureAwait(false);
    }

    protected override async Task<LoginValidateResult> ValidateLoginCoreAsync(Guid validationCode)
    {
        DynamicParameters param = new();
        param.Add("@ValidationCode", validationCode);

        LoginAttemptRow? attempt = await Db.GetZeroOrOneAsync<LoginAttemptRow>(
            "SELECT ID, SubmitDateUTC, EmailAddress FROM LoginAttempts WHERE ValidationCode = @ValidationCode AND ValidateDateUTC IS NULL LIMIT 1;",
            param
        ).ConfigureAwait(false);

        if (attempt is null)
        {
            return new LoginValidateResult { Success = false, EmailAddress = null, NotFound = true, Expired = false };
        }

        if (attempt.SubmitDateUTC < DateTime.UtcNow.AddMinutes(-5))
        {
            return new LoginValidateResult { Success = false, EmailAddress = attempt.EmailAddress, NotFound = false, Expired = true };
        }

        DynamicParameters updateParam = new();
        updateParam.Add("@ID", attempt.ID);
        updateParam.Add("@Now", DateTime.UtcNow);
        await Db.NonQueryAsync(
            "UPDATE LoginAttempts SET ValidateDateUTC = @Now WHERE ID = @ID;",
            updateParam
        ).ConfigureAwait(false);

        return new LoginValidateResult { Success = true, EmailAddress = attempt.EmailAddress, NotFound = false, Expired = false };
    }
}
