using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace SimpleSchedulerData
{
    public class SqliteDatabase
        : BaseDapperDatabase<SqliteConnection, SqliteCommand, SqliteTransaction>
    {
        public SqliteDatabase(IConfiguration config) : base(config) { }
    }
}
