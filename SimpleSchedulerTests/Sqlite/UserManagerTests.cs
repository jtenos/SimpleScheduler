using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleSchedulerBusiness;
using SimpleSchedulerBusiness.Sqlite;
using SimpleSchedulerData;
using SimpleSchedulerEmail;
using SimpleSchedulerEntities;
using SimpleSchedulerModels.Exceptions;

namespace SimpleSchedulerTests.Sqlite
{
    [TestClass]
    public class UserManagerTests
    {
        private readonly IConfiguration _config;
        private readonly string _databaseFileName;

        private const string VALID_EMAIL_ADDRESS = "joe@example.com";

        public UserManagerTests()
        {
            _config = GetServiceProvider().GetRequiredService<IConfiguration>();
            string connectionString = _config.GetConnectionString("SimpleScheduler");
            SqliteConnectionStringBuilder builder = new(connectionString);
            _databaseFileName = builder.DataSource;
        }

        private static IServiceProvider GetServiceProvider()
            => new ServiceCollection()
                .AddSingleton<IConfiguration>(
                    new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json")
                        .Build()
                )
                .AddScoped<IUserManager, UserManager>()
                .AddScoped<BaseDatabase, SqliteDatabase>()
                .AddScoped<DatabaseFactory>()
                .AddScoped<IEmailer, FakeEmailer>()
                .BuildServiceProvider();

        [TestInitialize]
        public async Task SetUpAsync()
        {
            if (File.Exists(_databaseFileName))
            {
                File.Delete(_databaseFileName);
                await Task.Delay(50);
            }
            await SqliteDatabase.CreateDatabaseAsync(_databaseFileName);
            await CreateUserAsync();
        }

        [TestMethod]
        public async Task UserManagerType()
        {
            var scope = GetServiceProvider().CreateScope();
            try
            {
                var userManager = scope.ServiceProvider.GetRequiredService<IUserManager>();
                Assert.IsInstanceOfType(userManager, typeof(UserManager));
            }
            finally
            {
                await ((IAsyncDisposable)scope).DisposeAsync();
            }
        }

        [TestMethod]
        public async Task LoginFailUserNotFound()
        {
            var scope = GetServiceProvider().CreateScope();
            try
            {
                var userManager = scope.ServiceProvider.GetRequiredService<IUserManager>();
                Guid guid = Guid.NewGuid();
                FakeEmailer.CurrentGuid = guid;
                var result = await userManager.LoginSubmitAsync("abc@def.ghi", default);
                Assert.IsFalse(result);
                Assert.IsFalse(FakeEmailer.Messages.TryGetValue(guid, out _));
            }
            finally
            {
                await ((IAsyncDisposable)scope).DisposeAsync();
            }
        }

        [TestMethod]
        public async Task LoginSuccess()
        {
            Guid guid = Guid.NewGuid();
            var scope = GetServiceProvider().CreateScope();
            try
            {
                var userManager = scope.ServiceProvider.GetRequiredService<IUserManager>();
                FakeEmailer.CurrentGuid = guid;
                var result = await userManager.LoginSubmitAsync(VALID_EMAIL_ADDRESS, default);
                Assert.IsTrue(result);
            }
            finally
            {
                await ((IAsyncDisposable)scope).DisposeAsync();
            }

            var (toAddresses, subject, bodyHTML) = FakeEmailer.Messages[guid];
            Assert.AreEqual(1, toAddresses.Count());
            Assert.AreEqual(VALID_EMAIL_ADDRESS, toAddresses.First());
            Assert.AreEqual("Scheduler (DEV) Log In", subject);
            var expectedHTML = new Regex(@"^\<a href\=\'http:\/\/localhost\:4200\/validate\-user\/[a-f0-9]{32}\' target\=_blank\>Click here to log in\<\/a\>$");
            Assert.IsTrue(expectedHTML.IsMatch(bodyHTML));
            var loginAttempt = await GetLoginAttemptAsync();
            Assert.AreEqual(VALID_EMAIL_ADDRESS, loginAttempt.EmailAddress);
            Guid.ParseExact(loginAttempt.ValidationKey, "N");
            var submitDate = DateTime.ParseExact(loginAttempt.SubmitDateUTC.ToString(), "yyyyMMddHHmmssfff", CultureInfo.InvariantCulture.DateTimeFormat);
            Assert.IsTrue(DateTime.UtcNow.Subtract(submitDate).TotalSeconds < 10);
        }

        [TestMethod]
        public async Task ValidateSuccess()
        {
            Guid guid = Guid.NewGuid();
            var scope = GetServiceProvider().CreateScope();
            try
            {
                var userManager = scope.ServiceProvider.GetRequiredService<IUserManager>();
                FakeEmailer.CurrentGuid = guid;
                var result = await userManager.LoginSubmitAsync(VALID_EMAIL_ADDRESS, default);
                string validationKey = Regex.Match(FakeEmailer.Messages[guid].bodyHTML, @"[a-f0-9]{32}").ToString();
                string validateResult = await userManager.LoginValidateAsync(validationKey, default);
                Assert.AreEqual(VALID_EMAIL_ADDRESS, validateResult);
            }
            finally
            {
                await ((IAsyncDisposable)scope).DisposeAsync();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidValidationKeyException))]
        public async Task ValidateCodeNotFound()
        {
            var scope = GetServiceProvider().CreateScope();
            try
            {
                var userManager = scope.ServiceProvider.GetRequiredService<IUserManager>();
                string validateResult = await userManager.LoginValidateAsync(Guid.NewGuid().ToString("N"), default);
            }
            finally
            {
                await ((IAsyncDisposable)scope).DisposeAsync();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ValidationKeyExpiredException))]
        public async Task ValidateCodeExpired()
        {
            Guid guid = Guid.NewGuid();
            var scope = GetServiceProvider().CreateScope();
            try
            {
                var userManager = scope.ServiceProvider.GetRequiredService<IUserManager>();
                FakeEmailer.CurrentGuid = guid;
                await userManager.LoginSubmitAsync(VALID_EMAIL_ADDRESS, default);
            }
            finally
            {
                await ((IAsyncDisposable)scope).DisposeAsync();
            }

            scope = GetServiceProvider().CreateScope();
            try
            {
                var userManager = scope.ServiceProvider.GetRequiredService<IUserManager>();
                string validationKey = Regex.Match(FakeEmailer.Messages[guid].bodyHTML, @"[a-f0-9]{32}").ToString();
                await SetValidationDateAsync(validationKey, DateTime.UtcNow.AddMinutes(-20));
                await userManager.LoginValidateAsync(validationKey, default);
            }
            finally
            {
                await ((IAsyncDisposable)scope).DisposeAsync();
            }
        }

        private async Task SetValidationDateAsync(string validationKey, DateTime submitDate)
        {
            using var conn = new SqliteConnection(_config.GetConnectionString("SimpleScheduler"));
            await conn.OpenAsync();
            using var comm = conn.CreateCommand();
            comm.CommandText = "UPDATE LoginAttempts SET SubmitDateUTC = @SubmitDateUTC WHERE ValidationKey = @ValidationKey;";
            comm.Parameters.AddRange(new[]
            {
                new SqliteParameter("@SubmitDateUTC", SqliteType.Integer) { Value = long.Parse(submitDate.ToString("yyyyMMddHHmmssfff")) },
                new SqliteParameter("@ValidationKey", SqliteType.Text) { Value = validationKey }
            });
            await comm.ExecuteNonQueryAsync();
        }

        private async Task<LoginAttemptEntity> GetLoginAttemptAsync()
        {
            using var conn = new SqliteConnection(_config.GetConnectionString("SimpleScheduler"));
            await conn.OpenAsync();
            using var comm = conn.CreateCommand();
            comm.CommandText = "SELECT * FROM LoginAttempts;";
            using var rdr = await comm.ExecuteReaderAsync();
            LoginAttemptEntity loginAttempt;
            Assert.IsTrue(await rdr.ReadAsync());
            Assert.IsTrue(await rdr.IsDBNullAsync(rdr.GetOrdinal("ValidationDateUTC")));
            loginAttempt = new(rdr.GetInt64(rdr.GetOrdinal("LoginAttemptID")),
                rdr.GetInt64(rdr.GetOrdinal("SubmitDateUTC")),
                rdr.GetString(rdr.GetOrdinal("EmailAddress")),
                rdr.GetString(rdr.GetOrdinal("ValidationKey")),
                ValidationDateUTC: default);
            Assert.IsFalse(await rdr.ReadAsync());
            return loginAttempt;
        }

        private async Task CreateUserAsync()
        {
            using var conn = new SqliteConnection(_config.GetConnectionString("SimpleScheduler"));
            await conn.OpenAsync();
            using var comm = conn.CreateCommand();
            comm.CommandText = "INSERT INTO Users (EmailAddress) VALUES (@EmailAddress);";
            comm.Parameters.Add(new SqliteParameter("@EmailAddress", SqliteType.Text) { Value = VALID_EMAIL_ADDRESS });
            await comm.ExecuteNonQueryAsync();
        }
    }
}
