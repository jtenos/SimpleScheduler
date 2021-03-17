using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace SimpleSchedulerData
{
    public class SqlDatabase
        : BaseDapperDatabase<SqlConnection, SqlCommand, SqlTransaction>
    {
        public SqlDatabase(IConfiguration config) : base(config) { }
    }
}
