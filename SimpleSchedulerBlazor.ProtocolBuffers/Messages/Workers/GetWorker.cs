using SimpleSchedulerModels;

namespace SimpleSchedulerBlazor.ProtocolBuffers.Messages.Workers;

partial class GetWorkerRequest
{
    public GetWorkerRequest(long id)
    {
        ID = id;
    }
}

partial class GetWorkerReply
{
    public GetWorkerReply(Worker worker)
    {
        Worker = new WorkerMessage(worker);
    }
}
