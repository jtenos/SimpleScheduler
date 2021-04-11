using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleSchedulerBusiness;
using SimpleSchedulerData;
using SimpleSchedulerModels;
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
        private readonly DatabaseFactory _databaseFactory;

        public JobsController(IJobManager jobManager, DatabaseFactory databaseFactory)
            => (_jobManager, _databaseFactory) = (jobManager, databaseFactory);

        [HttpGet("[action]")]
        public async Task<ImmutableArray<JobDetail>> GetJobs(CancellationToken cancellationToken,
            [FromQuery] long? workerID = null, [FromQuery] string? statusCode = null, [FromQuery] int pageNumber = 1)
            => await _jobManager.GetLatestJobsAsync(pageNumber, 100, statusCode, workerID, overdueOnly: false, cancellationToken);
    }
}
