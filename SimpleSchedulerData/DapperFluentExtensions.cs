using System;
using System.Data;
using Dapper;

namespace SimpleSchedulerData
{
    public static class DapperFluentExtensions
    {
        private static string GetParameterName(string paramName)
            => paramName.StartsWith("@") ? paramName : $"@{paramName}";

        private static DynamicParameters AddParameter(DynamicParameters dynamicParameters, string paramName, object? value, DbType dbType)
        {
            value ??= DBNull.Value;
            dynamicParameters.Add(GetParameterName(paramName), value, dbType, ParameterDirection.Input);
            return dynamicParameters;
        }

        public static DynamicParameters AddTimeParam(this DynamicParameters dynamicParameters, string paramName, TimeSpan value)
            => AddParameter(dynamicParameters, paramName, value, DbType.Time);
        public static DynamicParameters AddNullableTimeParam(this DynamicParameters dynamicParameters, string paramName, TimeSpan? value)
            => AddParameter(dynamicParameters, paramName, value, DbType.Time);
        public static DynamicParameters AddBitParam(this DynamicParameters dynamicParameters, string paramName, bool value)
            => AddParameter(dynamicParameters, paramName, value, DbType.Boolean);
        public static DynamicParameters AddIntParam(this DynamicParameters dynamicParameters, string paramName, int value)
            => AddParameter(dynamicParameters, paramName, value, DbType.Int32);
        public static DynamicParameters AddNullableIntParam(this DynamicParameters dynamicParameters, string paramName, int? value)
            => AddParameter(dynamicParameters, paramName, value, DbType.Int32);
        public static DynamicParameters AddDateTime2Param(this DynamicParameters dynamicParameters, string paramName, DateTime value)
            => AddParameter(dynamicParameters, paramName, value, DbType.DateTime2);
        public static DynamicParameters AddNullableDateTime2Param(this DynamicParameters dynamicParameters, string paramName, DateTime? value)
            => AddParameter(dynamicParameters, paramName, value, DbType.DateTime2);
        public static DynamicParameters AddNVarCharParam(this DynamicParameters dynamicParameters, string paramName, string value, int size)
            => AddParameter(dynamicParameters, paramName, new DbString { Value = value, IsAnsi = false, IsFixedLength = false, Length = size }, DbType.String);
        public static DynamicParameters AddNullableNVarCharParam(this DynamicParameters dynamicParameters, string paramName, string? value, int size)
            => AddParameter(dynamicParameters, paramName, new DbString { Value = value, IsAnsi = false, IsFixedLength = false, Length = size }, DbType.String);
        public static DynamicParameters AddNCharParam(this DynamicParameters dynamicParameters, string paramName, string value, int size)
            => AddParameter(dynamicParameters, paramName, new DbString { Value = value, IsAnsi = false, IsFixedLength = true, Length = size }, DbType.String);
        public static DynamicParameters AddNullableNCharParam(this DynamicParameters dynamicParameters, string paramName, string? value, int size)
            => AddParameter(dynamicParameters, paramName, new DbString { Value = value, IsAnsi = false, IsFixedLength = true, Length = size }, DbType.String);
        public static DynamicParameters AddUniqueIdentifierParam(this DynamicParameters dynamicParameters, string paramName, Guid value)
            => AddParameter(dynamicParameters, paramName, value, DbType.Guid);
        public static DynamicParameters AddNullableUniqueIdentifierParam(this DynamicParameters dynamicParameters, string paramName, Guid? value)
            => AddParameter(dynamicParameters, paramName, value, DbType.Guid);
    }
}
