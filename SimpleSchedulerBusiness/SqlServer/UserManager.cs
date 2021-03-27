using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SimpleSchedulerData;
using SimpleSchedulerEmail;
using SimpleSchedulerModels.Exceptions;

namespace SimpleSchedulerBusiness.SqlServer
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
            => await ScalarAsync<int>("SELECT COUNT(1) FROM dbo.[Users];",
            CreateDynamicParameters(), cancellationToken).ConfigureAwait(false);

        async Task<bool> IUserManager.LoginSubmitAsync(string emailAddress,
            CancellationToken cancellationToken)
        {
            bool emailFound = await ScalarAsync<bool>(@"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM dbo.[Users] WHERE EmailAddress = @EmailAddress
                ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END;
            ", CreateDynamicParameters()
                .AddNVarCharParam("@EmailAddress", emailAddress, 200),
                cancellationToken).ConfigureAwait(false);

            if (!emailFound) { return false; }

            Guid validationKey = await ScalarAsync<Guid>(@"
                DECLARE @Result TABLE (ValidationKey UNIQUEIDENTIFIER);

                INSERT dbo.LoginAttempts (SubmitDateUTC, EmailAddress)
                OUTPUT INSERTED.ValidationKey INTO @Result
                VALUES (@Now, @EmailAddress);

                SELECT ValidationKey FROM @Result;
            ", CreateDynamicParameters()
                .AddDateTime2Param("@Now", DateTime.UtcNow)
                .AddNVarCharParam("@EmailAddress", emailAddress, 200),
                cancellationToken).ConfigureAwait(false);

            string url = $"{_config["WebUrl"]}/validate-user/{validationKey}";
            await _emailer.SendEmailAsync(new[] { emailAddress },
                $"Scheduler ({_config["EnvironmentName"]}) Log In",
                $"<a href='{url}' target=_blank>Click here to log in</a>",
                cancellationToken);

            return true;
        }

        async Task<string> IUserManager.LoginValidateAsync(Guid validationKey, CancellationToken cancellationToken)
        {
            DateTime? submitDate = await ScalarAsync<DateTime>(@"
                SELECT SubmitDateUTC
                FROM dbo.LoginAttempts 
                WHERE ValidationKey = @ValidationKey
                AND ValidationDateUTC IS NULL;
            ",
                CreateDynamicParameters()
                .AddUniqueIdentifierParam("@ValidationKey", validationKey),
                cancellationToken).ConfigureAwait(false);

            if (!submitDate.HasValue || submitDate == default(DateTime)) { throw new InvalidValidationKeyException(); }
            if (submitDate < DateTime.UtcNow.AddMinutes(-5))
            {
                throw new ValidationKeyExpiredException();
            }

            return (await ScalarAsync<string>(@"
                DECLARE @Result TABLE(EmailAddress NVARCHAR(200));

                UPDATE dbo.LoginAttempts
                SET ValidationDateUTC = @Now
                OUTPUT INSERTED.EmailAddress INTO @Result
                WHERE ValidationKey = @ValidationKey;

                SELECT EmailAddress FROM @Result;
            ",
                CreateDynamicParameters()
                .AddDateTime2Param("@Now", DateTime.UtcNow)
                .AddUniqueIdentifierParam("@ValidationKey", validationKey),
                cancellationToken).ConfigureAwait(false))!;
        }
    }
}
