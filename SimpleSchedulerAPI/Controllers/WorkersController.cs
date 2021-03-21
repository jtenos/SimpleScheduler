using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleSchedulerBusiness;
using SimpleSchedulerModels;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleSchedulerAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize("ValidUser")]
    public class WorkersController : ControllerBase
    {
        private readonly IWorkerManager _workerManager;

        public WorkersController(IWorkerManager workerManager) => _workerManager = workerManager;

        [HttpGet]
        [Route("[action]")]
        public async Task<ImmutableArray<WorkerDetail>> GetAllWorkers(CancellationToken cancellationToken,
            bool getActive = true, bool getInactive = false)
            => await _workerManager.GetAllWorkerDetailsAsync(
                cancellationToken, getActive, getInactive);

        [HttpGet]
        [Route("[action]")]
        public async Task<Worker> GetWorker(int workerID, CancellationToken cancellationToken)
            => await _workerManager.GetWorkerAsync(workerID, cancellationToken);
    }
}
