namespace SimpleSchedulerBlazor.ProtocolBuffers.Types;

partial class NullableTimeSpan
{
    public NullableTimeSpan(TimeSpan? ts)
    {
        if (!ts.HasValue)
        {
            HasValue = false;
            Hour = Minute = Second = Millisecond = default;
            return;
        }

        Hour = ts.Value.Hours;
        Minute = ts.Value.Minutes;
        Second = ts.Value.Seconds;
        Millisecond = ts.Value.Milliseconds;
    }

    public TimeSpan? GetValueOrNull()
    {
        return HasValue ? new TimeSpan(Hour, Minute, Second, Millisecond) : null;
    }
}
