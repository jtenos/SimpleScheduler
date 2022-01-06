using System;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace SimpleSchedulerData
{
    public class SqlDatabase
        : BaseDatabase
    {
        public SqlDatabase(IConfiguration config)
            : base(config) { }

        protected override DbConnection GetConnection()
            => new SqlConnection
            {
                ConnectionString = Config.GetConnectionString("SimpleScheduler")
            };

        public override DbParameter GetInt64Parameter(string name, long? value)
            => new SqlParameter(name, SqlDbType.BigInt) { Value = value ?? (object)DBNull.Value };

        public override DbParameter GetStringParameter(string name, string? value, bool isFixed, int size)
            => new SqlParameter(name, isFixed ? SqlDbType.NChar : SqlDbType.NVarChar, size) { Value = value ?? (object)DBNull.Value };

        public override DbParameter GetBinaryParameter(string name, byte[]? value, bool isFixed, int size)
            => new SqlParameter(name, isFixed ? SqlDbType.Binary : SqlDbType.VarBinary, size) { Value = value ?? (object)DBNull.Value };

        public override string GetLastAutoIncrementQuery => "SELECT CAST(SCOPE_IDENTITY() AS BIGINT);";

        public override string GetOffsetLimitClause(int offset, int limit)
            => $" OFFSET {offset} ROWS FETCH NEXT {limit} ROWS ONLY ";
    }
}
