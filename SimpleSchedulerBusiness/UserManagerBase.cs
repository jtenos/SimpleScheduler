using System;
using System.Collections.Immutable;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SimpleSchedulerData;
using SimpleSchedulerEmail;
using SimpleSchedulerEntities;
using SimpleSchedulerModels.Exceptions;

namespace SimpleSchedulerBusiness
{
    public abstract class UserManagerBase
        : IUserManager
    {
        protected UserManagerBase(DatabaseFactory databaseFactory, IServiceProvider serviceProvider,
            IEmailer emailer, IConfiguration config)
            => (DatabaseFactory, ServiceProvider, Emailer, Config)
                = (databaseFactory, serviceProvider, emailer, config);

        protected DatabaseFactory DatabaseFactory { get; }
        protected IServiceProvider ServiceProvider { get; }
        protected IEmailer Emailer { get; }
        protected IConfiguration Config { get; }

        public virtual async Task<ImmutableArray<string>> GetAllUserEmailsAsync(CancellationToken cancellationToken)
        {
            if (!Config.GetValue<bool>("AllowLoginDropDown"))
            {
                return ImmutableArray<string>.Empty;
            }

            var db = await DatabaseFactory.GetDatabaseAsync(cancellationToken).ConfigureAwait(false);
            DbParameter[] parms = {};
            return (await db.GetManyAsync<string>(@"
                SELECT [EmailAddress] FROM [Users] ORDER BY [EmailAddress];
            ", parms, rdr => rdr.GetString(0), cancellationToken).ConfigureAwait(false)).ToImmutableArray();
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

            string url = $"{Config["WebUrl"]}/validate-user/{validationKey}";
            await Emailer.SendEmailAsync(new[] { emailAddress },
                $"Scheduler ({Config["EnvironmentName"]}) Log In",
                $"<a href='{url}' target=_blank>Click here to log in</a>",
                cancellationToken);

            return true;
        }

        public virtual async Task<string> LoginValidateAsync(string validationKey,
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

            if (!validateItems.Any()) { throw new InvalidValidationKeyException(); }

            if (DateTime.ParseExact(validateItems[0].SubmitDateUTC.ToString(), "yyyyMMddHHmmssfff", CultureInfo.InvariantCulture.DateTimeFormat)
                < DateTime.UtcNow.AddMinutes(-5))
            {
                throw new ValidationKeyExpiredException();
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
}
