using System.Collections.Immutable;
using System.Data.Common;
using System.Globalization;
using Microsoft.Extensions.Configuration;
using OneOf;
using OneOf.Types;
using SimpleSchedulerAppServices.Interfaces;
using SimpleSchedulerConfiguration.Models;
using SimpleSchedulerData;
using SimpleSchedulerEmail;
using SimpleSchedulerEntities;
using SimpleSchedulerModels.Configuration;
using SimpleSchedulerModels.ResultTypes;

namespace SimpleSchedulerAppServices.Implementations;

public sealed class UserManager
    : IUserManager
{
    protected UserManager(DatabaseFactory databaseFactory, IServiceProvider serviceProvider,
        IEmailer emailer, AppSettings appSettings)
    {
        DatabaseFactory = databaseFactory;
        ServiceProvider = serviceProvider;
        Emailer = emailer;
        AppSettings = appSettings;
    }

    protected DatabaseFactory DatabaseFactory { get; }
    protected IServiceProvider ServiceProvider { get; }
    protected IEmailer Emailer { get; }
    protected AppSettings AppSettings { get; }

    async Task<ImmutableArray<string>> IUserManager.GetAllUserEmailsAsync(CancellationToken cancellationToken)
    {
        if (!AppSettings.AllowLoginDropDown)
        {
            return ImmutableArray<string>.Empty;
        }

        return await _db.GetManyAsync<string>("[app].[Users_SelectAll]", cancellationToken).ConfigureAwait(false);
    }

    public virtual async Task<bool> LoginSubmitAsync(string emailAddress,
        CancellationToken cancellationToken)
    {
        var db = await DatabaseFactory.GetDatabaseAsync(cancellationToken).ConfigureAwait(false);
        DbParameter[] parms =
        {
            db.GetStringParameter("@EmailAddress", emailAddress, isFixed: false, size: 200)
        };
        var users = await db.GetManyAsync<UserEntity>(@"
            SELECT * FROM [Users] WHERE [EmailAddress] = @EmailAddress
        ", parms, Mapper.MapUser, cancellationToken).ConfigureAwait(false);

        if (!users.Any()) { return false; }

        string validationKey = Guid.NewGuid().ToString("N");
        parms = new[]
        {
            db.GetInt64Parameter("@SubmitDateUTC", DateTime.UtcNow),
            db.GetStringParameter("@EmailAddress", emailAddress, isFixed: false, size: 200),
            db.GetStringParameter("@ValidationKey", validationKey, isFixed: true, size: 32)
        };
        int recordsAffected = await db.NonQueryAsync(@"
            INSERT INTO LoginAttempts (SubmitDateUTC, EmailAddress, ValidationKey)
            VALUES (@SubmitDateUTC, @EmailAddress, @ValidationKey);
        ", parms, cancellationToken).ConfigureAwait(false);

        string url = $"{AppSettings.WebUrl}/validate-user/{validationKey}";
        await Emailer.SendEmailAsync(new[] { emailAddress },
            $"Scheduler ({AppSettings.EnvironmentName}) Log In",
            $"<a href='{url}' target=_blank>Click here to log in</a>",
            cancellationToken);

        return true;
    }

    public virtual async Task<OneOf<string, NotFound, Expired>> LoginValidateAsync(string validationKey,
        CancellationToken cancellationToken)
    {
        var db = await DatabaseFactory.GetDatabaseAsync(cancellationToken).ConfigureAwait(false);
        DbParameter[] parms =
        {
                db.GetStringParameter("@ValidationKey", validationKey, isFixed: true, size: 32)
            };
        var validateItems = await db.GetManyAsync(@"
                SELECT *
                FROM LoginAttempts
                WHERE ValidationKey = @ValidationKey
                AND ValidationDateUTC IS NULL;
            ", parms, Mapper.MapLoginAttempt, cancellationToken).ConfigureAwait(false);

        if (!validateItems.Any()) { return new NotFound(); }

        if (DateTime.ParseExact(validateItems[0].SubmitDateUTC.ToString(), "yyyyMMddHHmmssfff", CultureInfo.InvariantCulture.DateTimeFormat)
            < DateTime.UtcNow.AddMinutes(-5))
        {
            return new Expired();
        }

        parms = new[]
        {
            db.GetInt64Parameter("@ValidationDateUTC", DateTime.UtcNow),
            db.GetStringParameter("@ValidationKey", validationKey, isFixed: true, size: 32)
        };

        return await db.ScalarAsync<string>(@"
            UPDATE LoginAttempts
            SET ValidationDateUTC = @ValidationDateUTC
            WHERE ValidationKey = @ValidationKey;

            SELECT EmailAddress FROM LoginAttempts WHERE ValidationKey = @ValidationKey;
        ", parms, cancellationToken).ConfigureAwait(false)!;
    }
}
