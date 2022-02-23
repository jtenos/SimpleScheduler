using System.Globalization;

namespace SimpleSchedulerBlazor.ProtocolBuffers;

public static class DateTimeExtensions
{
    const string DATE_TIME_FORMAT = "yyyy\\-MM\\-ddTHH\\:mm\\:ss\\.fffZ";

    public static string AsIso8601(this DateTime dt)
    {
        return dt.ToString(DATE_TIME_FORMAT);
    }

    public static string AsIso8601(this DateTime? dt)
    {
        if (!dt.HasValue)
        {
            return "";
        }
        return dt.Value.AsIso8601();
    }

    public static DateTime? FromIso8601(this string s)
    {
        if (!DateTime.TryParseExact(s, DATE_TIME_FORMAT, provider: null, DateTimeStyles.None, out DateTime dt))
        {
            return null;
        }
        return dt;
    }
}
