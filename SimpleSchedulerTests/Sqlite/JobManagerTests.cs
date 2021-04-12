using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SimpleSchedulerTests.Sqlite
{
    [TestClass]
    public class JobManagerTests
        : JobManagerTestsBase
    {
        protected override void AddDatabaseSpecificServices(IServiceCollection sc)
            => SqliteTestSetup.AddDatabaseSpecificServices(sc);

        protected override void AddDatabaseSpecificConfig(IConfigurationBuilder configBuilder)
            => SqliteTestSetup.AddDatabaseSpecificConfig(configBuilder);

        [TestInitialize]
        public async Task SetUpAsync()
            => await SqliteTestSetup.SetUpAsync(Config);

        protected override DbConnection GetConnection()
            => SqliteTestSetup.GetConnection(Config.GetConnectionString("SimpleScheduler"));

        protected override DbParameter Int64Parameter(string name, long? value)
            => SqliteTestSetup.Int64Parameter(name, value);

        protected override DbParameter StringParameter(string name, string? value, int? size = null)
            => SqliteTestSetup.StringParameter(name, value, size);
    }
}
