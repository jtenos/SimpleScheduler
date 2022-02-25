using Dapper;
using System.Data;

namespace SimpleSchedulerData;

public static class DapperFluentExtensions
{
    private static DynamicParameters AddParameter(DynamicParameters dynamicParameters, 
        string paramName, object? value, DbType dbType)
    {
        value ??= DBNull.Value;
        dynamicParameters.Add(paramName, value, dbType, ParameterDirection.Input);
        return dynamicParameters;
    }

    // Bit:
    public static DynamicParameters AddBitParam(this DynamicParameters dynamicParameters, string paramName, bool value)
        => AddParameter(dynamicParameters, paramName, value, DbType.Boolean);
    public static DynamicParameters AddNullableBitParam(this DynamicParameters dynamicParameters, string paramName, bool? value)
        => AddParameter(dynamicParameters, paramName, value, DbType.Boolean);

    // Int32:
    public static DynamicParameters AddIntParam(this DynamicParameters dynamicParameters, string paramName, int value)
        => AddParameter(dynamicParameters, paramName, value, DbType.Int32);
    public static DynamicParameters AddNullableIntParam(this DynamicParameters dynamicParameters, string paramName, int? value)
        => AddParameter(dynamicParameters, paramName, value, DbType.Int32);

    // Int64:
    public static DynamicParameters AddLongParam(this DynamicParameters dynamicParameters, string paramName, long value)
        => AddParameter(dynamicParameters, paramName, value, DbType.Int64);
    public static DynamicParameters AddNullableLongParam(this DynamicParameters dynamicParameters, string paramName, long? value)
        => AddParameter(dynamicParameters, paramName, value, DbType.Int64);

    // DateTime:
    public static DynamicParameters AddDateTimeParam(this DynamicParameters dynamicParameters, string paramName, DateTime value)
        => AddParameter(dynamicParameters, paramName, value, DbType.DateTime);
    public static DynamicParameters AddNullableDateTimeParam(this DynamicParameters dynamicParameters, string paramName, DateTime? value)
        => AddParameter(dynamicParameters, paramName, value, DbType.DateTime);

    // DateTime2:
    public static DynamicParameters AddDateTime2Param(this DynamicParameters dynamicParameters, string paramName, DateTime value)
        => AddParameter(dynamicParameters, paramName, value, DbType.DateTime2);
    public static DynamicParameters AddNullableDateTime2Param(this DynamicParameters dynamicParameters, string paramName, DateTime? value)
        => AddParameter(dynamicParameters, paramName, value, DbType.DateTime2);

    // Time:
    public static DynamicParameters AddTimeParam(this DynamicParameters dynamicParameters, string paramName, TimeSpan value)
        => AddParameter(dynamicParameters, paramName, value, DbType.Time);
    public static DynamicParameters AddNullableTimeParam(this DynamicParameters dynamicParameters, string paramName, TimeSpan? value)
        => AddParameter(dynamicParameters, paramName, value, DbType.Time);

    // VarChar
    public static DynamicParameters AddVarCharParam(this DynamicParameters dynamicParameters, string paramName, string value, int size)
        => AddParameter(dynamicParameters, paramName, new DbString { Value = value, IsAnsi = true, IsFixedLength = false, Length = size }, DbType.String);
    public static DynamicParameters AddNullableVarCharParam(this DynamicParameters dynamicParameters, string paramName, string? value, int size)
        => AddParameter(dynamicParameters, paramName, new DbString { Value = value, IsAnsi = true, IsFixedLength = false, Length = size }, DbType.String);

    // NVarChar
    public static DynamicParameters AddNVarCharParam(this DynamicParameters dynamicParameters, string paramName, string value, int size)
        => AddParameter(dynamicParameters, paramName, new DbString { Value = value, IsAnsi = false, IsFixedLength = false, Length = size }, DbType.String);
    public static DynamicParameters AddNullableNVarCharParam(this DynamicParameters dynamicParameters, string paramName, string? value, int size)
        => AddParameter(dynamicParameters, paramName, new DbString { Value = value, IsAnsi = false, IsFixedLength = false, Length = size }, DbType.String);

    // Char
    public static DynamicParameters AddCharParam(this DynamicParameters dynamicParameters, string paramName, string value, int size)
        => AddParameter(dynamicParameters, paramName, new DbString { Value = value, IsAnsi = true, IsFixedLength = true, Length = size }, DbType.String);
    public static DynamicParameters AddNullableCharParam(this DynamicParameters dynamicParameters, string paramName, string? value, int size)
        => AddParameter(dynamicParameters, paramName, new DbString { Value = value, IsAnsi = true, IsFixedLength = true, Length = size }, DbType.String);

    // NChar
    public static DynamicParameters AddNCharParam(this DynamicParameters dynamicParameters, string paramName, string value, int size)
        => AddParameter(dynamicParameters, paramName, new DbString { Value = value, IsAnsi = false, IsFixedLength = true, Length = size }, DbType.String);
    public static DynamicParameters AddNullableNCharParam(this DynamicParameters dynamicParameters, string paramName, string? value, int size)
        => AddParameter(dynamicParameters, paramName, new DbString { Value = value, IsAnsi = false, IsFixedLength = true, Length = size }, DbType.String);

    // VarBinary (looks like Dapper can't handle BINARY parameter types, only VARBINARY)
    public static DynamicParameters AddVarBinaryParam(this DynamicParameters dynamicParameters, string paramName, byte[] value, int size)
    {
        dynamicParameters.Add(paramName, value, DbType.Binary, ParameterDirection.Input, size);
        return dynamicParameters;
    }
    public static DynamicParameters AddNullableVarBinaryParam(this DynamicParameters dynamicParameters, string paramName, string value, int size)
    {
        dynamicParameters.Add(paramName, value, DbType.Binary, ParameterDirection.Input, size);
        return dynamicParameters;
    }

    // Guid
    public static DynamicParameters AddUniqueIdentifierParam(this DynamicParameters dynamicParameters, string paramName, Guid value)
        => AddParameter(dynamicParameters, paramName, value, DbType.Guid);
    public static DynamicParameters AddNullableUniqueIdentifierParam(this DynamicParameters dynamicParameters, string paramName, Guid? value)
        => AddParameter(dynamicParameters, paramName, value, DbType.Guid);

    // XML
    public static DynamicParameters AddXmlParam(this DynamicParameters dynamicParameters, string paramName, string xml)
        => AddParameter(dynamicParameters, paramName, xml, DbType.Xml);
    public static DynamicParameters AddNullableXmlParam(this DynamicParameters dynamicParameters, string paramName, string? xml)
        => AddParameter(dynamicParameters, paramName, xml ?? (object)DBNull.Value, DbType.Xml);

    // [app].[BigIntArray]
    public static DynamicParameters AddBigIntArrayParam(this DynamicParameters dynamicParameters, string paramName, long[] values)
    {
        DataTable dt = new();
        dt.Columns.Add("IdentityVal", typeof(long));
        dt.Columns.Add("Value", typeof(long));
        for (int i = 0; i < values.Length; ++i)
        {
            DataRow row = dt.NewRow();
            row["IdentityVal"] = i + 1;
            row["Value"] = values[i];
            dt.Rows.Add(row);
        }

        dynamicParameters.Add(paramName, dt.AsTableValuedParameter("[app].[BigIntArray]"));

        return dynamicParameters;
    }
}
