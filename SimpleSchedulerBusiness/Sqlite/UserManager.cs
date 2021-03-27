using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SimpleSchedulerData;
using SimpleSchedulerEmail;
using SimpleSchedulerModels.Exceptions;

namespace SimpleSchedulerBusiness.Sqlite
{
    public class UserManager
        : BaseManager, IUserManager
    {
        private readonly IEmailer _emailer;
        private readonly IConfiguration _config;
        public UserManager(IDatabaseFactory databaseFactory, IServiceProvider serviceProvider,
            IEmailer emailer, IConfiguration config)
            : base(databaseFactory, serviceProvider) 
            => (_emailer, _config) = (emailer, config);

        async Task<int> IUserManager.CountUsersAsync(CancellationToken cancellationToken)
            => await ScalarAsync<int>("SELECT COUNT(1) FROM [Users];",
                CreateDynamicParameters(), cancellationToken).ConfigureAwait(false);

        // TODO: This shouldn't return the key - it should send the email and the controller
        // doesn't need to know about the key
        async Task<bool> IUserManager.LoginSubmitAsync(string emailAddress,
            CancellationToken cancellationToken)
        {
            bool emailFound = await ScalarAsync<bool>(@"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM [Users] WHERE [EmailAddress] = @EmailAddress
                ) THEN 1 ELSE 0 END;
            ", CreateDynamicParameters()
                .AddNVarCharParam("@EmailAddress", emailAddress, 200),
                cancellationToken).ConfigureAwait(false);

            if (!emailFound) { return false; }

            Guid validationKey = Guid.NewGuid();
            await NonQueryAsync(@"
                INSERT INTO [LoginAttempts] ([SubmitDateUTC], [EmailAddress], [ValidationKey])
                VALUES (@Now, @EmailAddress, @ValidationKey);
            ", CreateDynamicParameters()
                .AddDateTime2Param("@Now", DateTime.UtcNow)
                .AddNVarCharParam("@EmailAddress", emailAddress, 200)
                .AddNVarCharParam("@ValidationKey", validationKey.ToString("N"), 32),
                cancellationToken).ConfigureAwait(false);

            string url = $"{_config["WebUrl"]}/validate-user/{validationKey}";
            await _emailer.SendEmailAsync(new[] { emailAddress },
                $"Scheduler ({_config["EnvironmentName"]}) Log In",
                $"<a href='{url}' target=_blank>Click here to log in</a>",
                cancellationToken);

            return true;
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
