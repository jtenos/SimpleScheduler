namespace SimpleSchedulerBlazor.ProtocolBuffers.Types;

partial class NullableString
{
    public NullableString(string? s)
    {
        HasValue = s is not null;
        Value = s ?? "";
    }

    public string? GetValueOrNull()
    {
        return HasValue ? Value : null;
    }
}
