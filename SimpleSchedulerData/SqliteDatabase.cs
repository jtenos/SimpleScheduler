using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace SimpleSchedulerData
{
    public class SqliteDatabase
        : BaseDatabase<SqliteConnection, SqliteTransaction, SqliteParameter, SqliteDataReader>
    {
        public SqliteDatabase(IConfiguration config)
            : base(config) { }
    }
}
