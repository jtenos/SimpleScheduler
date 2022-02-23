using SimpleSchedulerModels;

namespace SimpleSchedulerBlazor.ProtocolBuffers.Messages.Workers;

partial class GetAllWorkersRequest
{

}

partial class GetAllWorkersReply
{
    public GetAllWorkersReply(IEnumerable<Worker> workers)
    {
        foreach (Worker worker in workers)
        {
            Workers.Add(new WorkerMessage(worker));
        }
    }
}
