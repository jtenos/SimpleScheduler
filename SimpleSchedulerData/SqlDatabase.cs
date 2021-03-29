using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace SimpleSchedulerData
{
    public class SqlDatabase
        : BaseDatabase<SqlConnection, SqlTransaction, SqlParameter, SqlDataReader>
    {
        public SqlDatabase(IConfiguration config) : base(config) { }
    }
}
