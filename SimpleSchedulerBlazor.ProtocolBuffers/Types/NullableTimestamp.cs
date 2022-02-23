using Google.Protobuf.WellKnownTypes;

namespace SimpleSchedulerBlazor.ProtocolBuffers.Types;

partial class NullableTimestamp
{
    public NullableTimestamp(DateTime? dt)
    {
        HasValue = dt.HasValue;
        Value = Timestamp.FromDateTime(dt ?? default);
    }

    public DateTime? GetValueOrNull()
    {
        return HasValue ? Value.ToDateTime() : default;
    }
}
