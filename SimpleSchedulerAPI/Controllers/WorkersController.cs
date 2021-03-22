using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleSchedulerBusiness;
using SimpleSchedulerModels;
using SimpleSchedulerModels.Exceptions;
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
            bool getActive = true, bool getInactive = true)
            => await _workerManager.GetAllWorkerDetailsAsync(
                cancellationToken, getActive, getInactive);

        [HttpGet]
        [Route("[action]")]
        public async Task<Worker> GetWorker(int workerID, CancellationToken cancellationToken)
            => await _workerManager.GetWorkerAsync(workerID, cancellationToken);

        [HttpPost]
        [Route("[action]")]
        public async Task DeleteWorker([FromBody] DeleteWorkerRequest deleteWorkerRequest,
            CancellationToken cancellationToken)
            => await _workerManager.DeactivateWorkerAsync(deleteWorkerRequest.WorkerID, cancellationToken);

        [HttpPost]
        [Route("[action]")]
        public async Task ReactivateWorker([FromBody] ReactivateWorkerRequest reactivateWorkerRequest,
            CancellationToken cancellationToken)
            => await _workerManager.ReactivateWorkerAsync(reactivateWorkerRequest.WorkerID, cancellationToken);

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> SaveWorker(Worker worker, CancellationToken cancellationToken)
        {
            try
            {
                if (worker.WorkerID > 0)
                {
                    await _workerManager.UpdateWorkerAsync(worker.WorkerID, worker.IsActive, worker.WorkerName,
                        worker.DetailedDescription, worker.EmailOnSuccess, worker.ParentWorkerID,
                        worker.TimeoutMinutes, worker.OverdueMinutes, worker.DirectoryName,
                        worker.Executable, worker.ArgumentValues, cancellationToken);
                    return Ok();
                }

                await _workerManager.AddWorkerAsync(worker.IsActive, worker.WorkerName,
                        worker.DetailedDescription, worker.EmailOnSuccess, worker.ParentWorkerID,
                        worker.TimeoutMinutes, worker.OverdueMinutes, worker.DirectoryName,
                        worker.Executable, worker.ArgumentValues, cancellationToken);
                return Ok();
            }
            catch (WorkerAlreadyExistsException)
            {
                return BadRequest($"Worker with name {worker.WorkerName} already exists");
            }
            catch (CircularWorkerRelationshipException)
            {
                return BadRequest($"This would create a circular relationship (parent->child->parent)");
            }
            catch
            {
                return BadRequest("Unknown error, please try again");
            }
        }

        public record DeleteWorkerRequest(int WorkerID);
        public record ReactivateWorkerRequest(int WorkerID);
    }
}
