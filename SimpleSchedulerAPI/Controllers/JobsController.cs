using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleSchedulerBusiness;
using SimpleSchedulerData;
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
    public class JobsController : ControllerBase
    {
        private readonly IJobManager _jobManager;

        public JobsController(IJobManager jobManager)
            => _jobManager = jobManager;

        [HttpGet("[action]")]
        public async Task<ImmutableArray<JobDetail>> GetJobs(CancellationToken cancellationToken,
            [FromQuery] long? workerID = null, [FromQuery] string? statusCode = null, [FromQuery] int pageNumber = 1)
            => await _jobManager.GetLatestJobsAsync(pageNumber, 100, statusCode, workerID, overdueOnly: false, cancellationToken);

        [HttpPost("[action]")]
        public async Task<IActionResult> CancelJob([FromBody] CancelJobRequest cancelRequest, CancellationToken cancellationToken)
        {
            try
            {
                await _jobManager.CancelJobAsync(cancelRequest.JobID, cancellationToken);
                return Ok();
            }
            catch (JobAlreadyRunningException)
            {
                return BadRequest(new { message = "Job is already running, unable to cancel" });
            }
            catch (JobAlreadyCompletedException)
            {
                return BadRequest(new { message = "Job has already completed, unable to cancel" });
            }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> AcknowledgeError([FromBody] AcknowledgeErrorRequest ackRequest, CancellationToken cancellationToken)
        {
            await _jobManager.AcknowledgeErrorAsync(ackRequest.JobID, cancellationToken);
            return Ok();
        }

        [HttpPost("[action]")]
        public async Task<object> GetDetailedMessage([FromBody] GetDetailedMessageRequest request, CancellationToken cancellationToken)
        {
            string message = await _jobManager.GetDetailedMessageAsync(request.JobID, cancellationToken);
            return Ok(new { message });
        }

        public record CancelJobRequest(long JobID);
        public record AcknowledgeErrorRequest(long JobID);
        public record GetDetailedMessageRequest(long JobID);
    }
}
