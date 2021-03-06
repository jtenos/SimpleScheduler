using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.Common;
using System.Threading.Tasks;

namespace SimpleSchedulerTests.SqlServer
{
    [TestClass]
    public class ScheduleManagerTests
        : ScheduleManagerTestsBase
    {
        protected override void AddDatabaseSpecificServices(IServiceCollection sc)
            => SqlServerTestSetup.AddDatabaseSpecificServices(sc);

        protected override void AddDatabaseSpecificConfig(IConfigurationBuilder configBuilder)
            => SqlServerTestSetup.AddDatabaseSpecificConfig(configBuilder);

        [TestInitialize]
        public async Task SetUpAsync()
            => await SqlServerTestSetup.SetUpAsync(Config);

        protected override DbConnection GetConnection()
            => SqlServerTestSetup.GetConnection(Config.GetConnectionString("SimpleScheduler"));

        protected override DbParameter Int64Parameter(string name, long? value)
            => SqlServerTestSetup.Int64Parameter(name, value);
    }
}
