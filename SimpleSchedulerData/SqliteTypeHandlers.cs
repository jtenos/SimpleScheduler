using Dapper;
using System.Data;
using System.Globalization;

namespace SimpleSchedulerData;

/// <summary>
/// Dapper type handlers that store/read <see cref="DateTime"/>, <see cref="TimeSpan"/> and
/// <see cref="Guid"/> as TEXT, so SQLite holds ISO-8601 strings (per issue #63) that work with
/// SQLite's date/time functions. Registering a handler for a type also covers its nullable form.
/// </summary>
public static class SqliteTypeHandlers
{
    private static bool _registered;
    private static readonly object _lock = new();

    public static void Register()
    {
        lock (_lock)
        {
            if (_registered) { return; }

            SqlMapper.AddTypeHandler(new DateTimeHandler());
            SqlMapper.AddTypeHandler(new TimeSpanHandler());
            SqlMapper.AddTypeHandler(new GuidHandler());
            SqlMapper.AddTypeHandler(new BoolHandler());
            SqlMapper.AddTypeHandler(new IntHandler());

            _registered = true;
        }
    }

    private sealed class DateTimeHandler : SqlMapper.TypeHandler<DateTime>
    {
        public override void SetValue(IDbDataParameter parameter, DateTime value)
        {
            DateTime utc = value.Kind == DateTimeKind.Local ? value.ToUniversalTime() : value;
            parameter.Value = utc.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ", CultureInfo.InvariantCulture);
        }

        public override DateTime Parse(object value)
            => DateTime.Parse((string)value, CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
    }

    private sealed class TimeSpanHandler : SqlMapper.TypeHandler<TimeSpan>
    {
        public override void SetValue(IDbDataParameter parameter, TimeSpan value)
            => parameter.Value = value.ToString("c", CultureInfo.InvariantCulture);

        public override TimeSpan Parse(object value)
            => TimeSpan.Parse((string)value, CultureInfo.InvariantCulture);
    }

    private sealed class GuidHandler : SqlMapper.TypeHandler<Guid>
    {
        public override void SetValue(IDbDataParameter parameter, Guid value)
            => parameter.Value = value.ToString();

        public override Guid Parse(object value)
            => Guid.Parse((string)value);
    }

    // SQLite stores booleans as INTEGER 0/1. Dapper cannot convert Int64 to a bool constructor
    // parameter on its own (the entity records use bool), so this handler does it explicitly.
    private sealed class BoolHandler : SqlMapper.TypeHandler<bool>
    {
        public override void SetValue(IDbDataParameter parameter, bool value)
            => parameter.Value = value ? 1 : 0;

        public override bool Parse(object value)
            => Convert.ToInt64(value) != 0;
    }

    // SQLite returns every integer as Int64; entity records that use int (e.g. TimeoutMinutes)
    // need this to convert via a constructor parameter.
    private sealed class IntHandler : SqlMapper.TypeHandler<int>
    {
        public override void SetValue(IDbDataParameter parameter, int value)
            => parameter.Value = value;

        public override int Parse(object value)
            => Convert.ToInt32(value);
    }
}
