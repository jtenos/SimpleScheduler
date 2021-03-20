using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using SimpleSchedulerData;
using SimpleSchedulerModels.Exceptions;

namespace SimpleSchedulerBusiness
{
    public class UserManager
        : BaseManager, IUserManager
    {
        public UserManager(IDatabaseFactory databaseFactory, IServiceProvider serviceProvider, IMemoryCache cache)
            : base(databaseFactory, serviceProvider, cache) { }

        async Task<(bool EmailFound, string ValidationKey)> IUserManager.LoginSubmitAsync(string emailAddress,
            CancellationToken cancellationToken)
        {
            bool emailFound = await ScalarAsync<bool>(@"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM dbo.[Users] WHERE EmailAddress = @EmailAddress
                ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END;
            ", CreateDynamicParameters()
                .AddNVarCharParam("@EmailAddress", emailAddress, 200),
                cancellationToken).ConfigureAwait(false);

            if (!emailFound) { return (emailFound, ""); }

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

            // TODO: Send user the email

            return (emailFound, validationKey.ToString("N"));
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
