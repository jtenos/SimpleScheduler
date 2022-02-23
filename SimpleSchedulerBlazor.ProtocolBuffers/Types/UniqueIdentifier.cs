namespace SimpleSchedulerBlazor.ProtocolBuffers.Types;

partial class UniqueIdentifier
{
    public UniqueIdentifier(Guid g)
    {
        Value = g.ToString("N");
    }
}
