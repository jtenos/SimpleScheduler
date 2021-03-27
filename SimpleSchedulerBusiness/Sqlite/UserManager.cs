using System;
using System.Threading;
using System.Threading.Tasks;
using SimpleSchedulerData;
using SimpleSchedulerModels.Exceptions;

namespace SimpleSchedulerBusiness.Sqlite
{
    public class UserManager
        : BaseManager, IUserManager
    {
        public UserManager(IDatabaseFactory databaseFactory, IServiceProvider serviceProvider)
            : base(databaseFactory, serviceProvider) { }

        async Task<int> IUserManager.CountUsersAsync(CancellationToken cancellationToken)
            => await ScalarAsync<int>("SELECT COUNT(1) FROM [Users];",
                CreateDynamicParameters(), cancellationToken).ConfigureAwait(false);

        // TODO: This shouldn't return the key - it should send the email and the controller
        // doesn't need to know about the key
        async Task<(bool EmailFound, string ValidationKey)> IUserManager.LoginSubmitAsync(string emailAddress,
            CancellationToken cancellationToken)
        {
            bool emailFound = await ScalarAsync<bool>(@"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM [Users] WHERE [EmailAddress] = @EmailAddress
                ) THEN 1 ELSE 0 END;
            ", CreateDynamicParameters()
                .AddNVarCharParam("@EmailAddress", emailAddress, 200),
                cancellationToken).ConfigureAwait(false);

            if (!emailFound) { return (emailFound, ""); }

            Guid validationKey = Guid.NewGuid();
            await NonQueryAsync(@"
                INSERT INTO [LoginAttempts] ([SubmitDateUTC], [EmailAddress], [ValidationKey])
                VALUES (@Now, @EmailAddress, @ValidationKey);
            ", CreateDynamicParameters()
                .AddDateTime2Param("@Now", DateTime.UtcNow)
                .AddNVarCharParam("@EmailAddress", emailAddress, 200)
                .AddNVarCharParam("@ValidationKey", validationKey.ToString("N"), 32),
                cancellationToken).ConfigureAwait(false);

            // TODO: move the email send code here from the controller

            return (emailFound, validationKey.ToString("N"));
        }

        public record LoginValidateItem(DateTimeOffset SubmitDateUTC, string EmailAddress);

        async Task<string> IUserManager.LoginValidateAsync(Guid validationKey, CancellationToken cancellationToken)
        {
            var validateItem = await GetOneAsync<LoginValidateItem>(@"
                SELECT [SubmitDateUTC], [EmailAddress]
                FROM [LoginAttempts]
                WHERE [ValidationKey] = @ValidationKey
                AND [ValidationDateUTC] IS NULL;
            ",
                CreateDynamicParameters()
                .AddNVarCharParam("@ValidationKey", validationKey.ToString("N"), 32),
                cancellationToken).ConfigureAwait(false);

            if (validateItem == null) { throw new InvalidValidationKeyException(); }

            if (validateItem.SubmitDateUTC < DateTime.UtcNow.AddMinutes(-5))
            {
                throw new ValidationKeyExpiredException();
            }

            return await ScalarAsync<string>(@"
                UPDATE [LoginAttempts]
                SET [ValidationDateUTC] = @Now
                WHERE [ValidationKey] = @ValidationKey;

                SELECT EmailAddress FROM [LoginAttempts] WHERE ValidationKey = @ValidationKey;
            ",
                CreateDynamicParameters()
                .AddDateTime2Param("@Now", DateTime.UtcNow)
                .AddUniqueIdentifierParam("@ValidationKey", validationKey),
                cancellationToken).ConfigureAwait(false)!;
        }
    }
}
