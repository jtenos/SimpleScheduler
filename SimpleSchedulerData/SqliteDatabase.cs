using System;
using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace SimpleSchedulerData
{
    public class SqliteDatabase
        : BaseDatabase
    {
        public SqliteDatabase(IConfiguration config)
            : base(config) { }

        protected override DbConnection GetConnection()
            => new SqliteConnection
            {
                ConnectionString = Config.GetConnectionString("SimpleScheduler")
            };


        public override DbParameter GetInt64Parameter(string name, long? value)
            => new SqliteParameter(name, SqliteType.Integer) { Value = value ?? (object)DBNull.Value };

        public override DbParameter GetStringParameter(string name, string? value, bool isFixed, int size)
            => new SqliteParameter(name, SqliteType.Text) { Value = value ?? (object)DBNull.Value };

        public override string GetLastAutoIncrementQuery => "SELECT last_insert_rowid();";

        public override string GetOffsetLimitClause(int offset, int limit)
            => $" LIMIT {limit} OFFSET {offset} ";
    }
}
