namespace SimpleSchedulerBlazorWasm;

public static class FormatExtensions
{
    public static string FormatDate(this DateTime? dt) => dt.HasValue ? FormatDate(dt.Value) : "";
    public static string FormatDate(this DateTime dt) => dt.ToString("MMM dd yyyy");
    public static string FormatTime(this DateTime? dt) => dt.HasValue ? FormatTime(dt.Value) : "";
    public static string FormatTime(this DateTime dt) => $"{dt:HH\\:mm\\:ss} (UTC)";
}
