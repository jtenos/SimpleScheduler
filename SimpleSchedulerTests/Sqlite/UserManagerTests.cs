using System;
using System.IO;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleSchedulerBusiness;
using SimpleSchedulerBusiness.Sqlite;
using SimpleSchedulerData;

namespace SimpleSchedulerTests
{
    [TestClass]
    public class UserManagerTests
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IUserManager _userManager;

        public UserManagerTests()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            _serviceProvider = new ServiceCollection()
                .AddSingleton<IConfiguration>(config)
                .AddScoped<IUserManager, UserManager>()
                .AddScoped<BaseDatabase, SqliteDatabase>()
                .AddScoped<DatabaseFactory>()
                .BuildServiceProvider();
            _userManager = _serviceProvider.GetRequiredService<IUserManager>();
        }

        [TestInitialize]
        public void SetUp()
        {
            string connectionString = _serviceProvider.GetRequiredService<IConfiguration>()
                .GetConnectionString("SimpleScheduler");
            SqliteConnectionStringBuilder builder = new(connectionString);
            builder.
        }

        [TestMethod]
        public void TestMethod1()
        {
        }
    }
}
