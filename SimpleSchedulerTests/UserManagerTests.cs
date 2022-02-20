using System.Data;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneOf.Types;
using SimpleSchedulerAppServices.Implementations;
using SimpleSchedulerAppServices.Interfaces;
using SimpleSchedulerConfiguration.Models;
using SimpleSchedulerData;
using SimpleSchedulerEmail;
using SimpleSchedulerModels.ResultTypes;

namespace SimpleSchedulerTests;

public class UserManagerTests
{
    private const string VALID_EMAIL_ADDRESS = "joe@example.com";

    private readonly IServiceProvider _serviceProvider;

    public UserManagerTests()
    {
        AppSettings appSettings = new()
        {
            AllowLoginDropDown = true,
            ConnectionString = @"Server=(localdb)\MSSqlLocalDb;Database=Scheduler_UnitTests;Integrated Security=True;",
            EnvironmentName = "UnitTests",
            Jwt = new() { Audience = "", Issuer = "", Key = "" },
            MailSettings = new(),
            WebUrl = "http://localhost",
            WorkerPath = ""
        };

        _serviceProvider = new ServiceCollection()
            .AddSingleton(appSettings)
            .AddScoped<IEmailer, FakeEmailer>()
            .AddScoped<IUserManager, UserManager>()
            .AddScoped<SqlDatabase>()
            .BuildServiceProvider();
    }

    [TestInitialize]
    public async Task InitializeAsync()
    {
        using AsyncServiceScope scope = _serviceProvider.CreateAsyncScope();
        AppSettings appSettings = scope.ServiceProvider.GetRequiredService<AppSettings>();
        using SqlConnection conn = new(appSettings.ConnectionString);
        await conn.OpenAsync();
        using SqlCommand comm = conn.CreateCommand();
        comm.CommandText = @"
            DELETE FROM [app].[Users];
            DELETE FROM [app].[LoginAttempts];
            DELETE FROM [app].[Jobs];
            DELETE FROM [app].[Schedules];
            DELETE FROM [app].[Workers];

            INSERT INTO [app].[Users] ([EmailAddress]) VALUES ('joe@example.com');
        ";
        await comm.ExecuteNonQueryAsync();
    }

    [TestMethod]
    public async Task TestLoginFailUserNotFound()
    {
        using AsyncServiceScope scope = _serviceProvider.CreateAsyncScope();
        IUserManager userManager = scope.ServiceProvider.GetRequiredService<IUserManager>();
        FakeEmailer emailer = (FakeEmailer)scope.ServiceProvider.GetRequiredService<IEmailer>();

        bool success = await userManager.LoginSubmitAsync("abc@def.ghi", default);
        Assert.IsFalse(success);
        Assert.AreEqual(0, emailer.Messages.Count);
    }

    [TestMethod]
    public async Task TestLoginSuccess()
    {
        using AsyncServiceScope scope = _serviceProvider.CreateAsyncScope();
        IUserManager userManager = scope.ServiceProvider.GetRequiredService<IUserManager>();
        FakeEmailer emailer = (FakeEmailer)scope.ServiceProvider.GetRequiredService<IEmailer>();

        bool success = await userManager.LoginSubmitAsync(VALID_EMAIL_ADDRESS, default);
        Assert.IsTrue(success);
        Assert.AreEqual(1, emailer.Messages.Count);
        Assert.AreEqual("Scheduler (UnitTests) Log In", emailer.Messages[0].subject);

        Regex expectedHTML = new(@"^\<a href\=\'http:\/\/localhost\/validate\-user\/[a-f0-9]{32}\' target\=_blank\>Click here to log in\<\/a\>$");
        Assert.IsTrue(expectedHTML.IsMatch(emailer.Messages[0].bodyHTML));

        Assert.AreEqual(1, emailer.Messages[0].toAddresses.Length);
        Assert.AreEqual(VALID_EMAIL_ADDRESS, emailer.Messages[0].toAddresses[0]);

        AppSettings appSettings = scope.ServiceProvider.GetRequiredService<AppSettings>();
        using SqlConnection conn = new(appSettings.ConnectionString);
        await conn.OpenAsync();
        using SqlCommand comm = conn.CreateCommand();
        comm.CommandText = @"
            SELECT * FROM [app].[LoginAttempts];
        ";
        using SqlDataReader rdr = await comm.ExecuteReaderAsync();

        Assert.IsTrue(await rdr.ReadAsync());
        DateTime submitDateUTC = rdr.GetDateTime("SubmitDateUTC");
        string emailAddress = rdr.GetString("EmailAddress");
        Guid validationCode = rdr.GetGuid("ValidationCode");
        DateTime? validateDate = rdr.IsDBNull("ValidateDateUTC") ? null : rdr.GetDateTime("ValidateDateUTC");

        AssertDatesClose(submitDateUTC, DateTime.UtcNow);
        Assert.AreEqual(VALID_EMAIL_ADDRESS, emailAddress);
        Assert.IsNull(validateDate);

        Assert.IsFalse(await rdr.ReadAsync());
    }

    [TestMethod]
    public async Task TestValidateSuccess()
    {
        using AsyncServiceScope scope = _serviceProvider.CreateAsyncScope();
        IUserManager userManager = scope.ServiceProvider.GetRequiredService<IUserManager>();
        FakeEmailer emailer = (FakeEmailer)scope.ServiceProvider.GetRequiredService<IEmailer>();

        await userManager.LoginSubmitAsync(VALID_EMAIL_ADDRESS, default);
        Guid validationCode = Guid.Parse(emailer.Messages[0].subject
            .Replace(@"<a href='http://localhost/validate-user/", "")
            .Replace("' target=_blank>Click here to log in</a>", "")
        );

        var validateResult = await userManager.LoginValidateAsync(validationCode, default);

        validateResult.Switch(
            (string emailAddress) =>
            {
                AppSettings appSettings = scope.ServiceProvider.GetRequiredService<AppSettings>();
                using SqlConnection conn = new(appSettings.ConnectionString);
                conn.Open();
                using SqlCommand comm = conn.CreateCommand();
                comm.CommandText = @"
                    SELECT * FROM [app].[LoginAttempts];
                ";
                using SqlDataReader rdr = comm.ExecuteReader();

                Assert.IsTrue(rdr.Read());
                DateTime? validateDate = rdr.IsDBNull("ValidateDateUTC") ? null : rdr.GetDateTime("ValidateDateUTC");
                AssertDatesClose(DateTime.UtcNow, validateDate!.Value);

                Assert.IsFalse(rdr.Read());
            },
            (NotFound notFound) =>
            {
                Assert.Fail("Validation not found");
            },
            (Expired expired) =>
            {
                Assert.Fail("Validation expired");
            }
        );
    }

    [TestMethod]
    public async Task TestValidateCodeNotFound()
    {
        using AsyncServiceScope scope = _serviceProvider.CreateAsyncScope();
        IUserManager userManager = scope.ServiceProvider.GetRequiredService<IUserManager>();

        var validateResult = await userManager.LoginValidateAsync(Guid.NewGuid(), default);

        validateResult.Switch(
            (string emailAddress) =>
            {
                Assert.Fail("Should have failed for validation not found");
            },
            (NotFound notFound) =>
            {
                // This is the expected result
            },
            (Expired expired) =>
            {
                Assert.Fail("Validation expired");
            }
        );
    }

    [TestMethod]
    public async Task ValidateCodeExpired()
    {
        using AsyncServiceScope scope = _serviceProvider.CreateAsyncScope();
        IUserManager userManager = scope.ServiceProvider.GetRequiredService<IUserManager>();

        AppSettings appSettings = scope.ServiceProvider.GetRequiredService<AppSettings>();
        using (SqlConnection conn = new(appSettings.ConnectionString))
        {
            await conn.OpenAsync();
            using SqlCommand comm = conn.CreateCommand();
            comm.CommandText = @"
                UPDATE [app].[LoginAttempts]
                SET [SubmitDateUTC] = DATEADD(MINUTE, 20, [SubmitDateUTC]);
            ";
            await comm.ExecuteNonQueryAsync();
        }

        var validateResult = await userManager.LoginValidateAsync(Guid.NewGuid(), default);

        validateResult.Switch(
            (string emailAddress) =>
            {
                Assert.Fail("Should have failed for validation not found");
            },
            (NotFound notFound) =>
            {
                Assert.Fail("Validation not found");
            },
            (Expired expired) =>
            {
                // This is the expected result
            }
        );
    }

    private static void AssertDatesClose(DateTime dt1, DateTime dt2)
    {
        double diff = Math.Abs(dt1.Subtract(dt2).TotalMilliseconds);
        Assert.IsTrue(diff < 10000);
    }
}
