using Google.Protobuf.WellKnownTypes;

namespace SimpleSchedulerBlazor.ProtocolBuffers.Types;

partial class NullableInt64
{
    public NullableInt64(long? l)
    {
        HasValue = l.HasValue;
        Value = l ?? default;
    }

    public long? GetValueOrNull()
    {
        return HasValue ? Value : null;
    }
}
