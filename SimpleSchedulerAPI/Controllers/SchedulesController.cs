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
    public class SchedulesController : ControllerBase
    {
        private readonly IScheduleManager _scheduleManager;
        private readonly DatabaseFactory _databaseFactory;

        public SchedulesController(IScheduleManager scheduleManager, DatabaseFactory databaseFactory)
            => (_scheduleManager, _databaseFactory) = (scheduleManager, databaseFactory);

        [HttpGet]
        [Route("[action]")]
        public async Task<ImmutableArray<ScheduleDetail>> GetAllSchedules(CancellationToken cancellationToken,
            bool getActive = true, bool getInactive = true)
            => await _scheduleManager.GetAllSchedulesAsync(cancellationToken, getActive, getInactive);

        [HttpGet]
        [Route("[action]")]
        public async Task<ScheduleDetail> GetSchedule(long scheduleID, CancellationToken cancellationToken)
            => await _scheduleManager.GetScheduleAsync(scheduleID, cancellationToken);

        [HttpPost]
        [Route("[action]")]
        public async Task DeleteSchedule([FromBody] DeleteScheduleRequest deleteScheduleRequest,
            CancellationToken cancellationToken)
            => await _scheduleManager.DeactivateScheduleAsync(deleteScheduleRequest.ScheduleID, cancellationToken);

        [HttpPost]
        [Route("[action]")]
        public async Task ReactivateSchedule([FromBody] ReactivateScheduleRequest reactivateScheduleRequest,
            CancellationToken cancellationToken)
            => await _scheduleManager.ReactivateScheduleAsync(reactivateScheduleRequest.ScheduleID, cancellationToken);

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> SaveSchedule(Schedule schedule, CancellationToken cancellationToken)
        {
            try
            {
                if (schedule.ScheduleID > 0)
                {
                    await _scheduleManager.UpdateScheduleAsync(schedule.ScheduleID,
                        schedule.WorkerID, schedule.Sunday, schedule.Monday, schedule.Tuesday,
                        schedule.Wednesday, schedule.Thursday, schedule.Friday, schedule.Saturday,
                        schedule.TimeOfDayUTC.AsTimeSpan(), schedule.RecurTime.AsTimeSpan(),
                        schedule.RecurBetweenStartUTC.AsTimeSpan(),
                        schedule.RecurBetweenEndUTC.AsTimeSpan(), cancellationToken);
                    return Ok(new { Success = true });
                }

                await _scheduleManager.AddScheduleAsync(schedule.WorkerID, isActive: true, schedule.Sunday, schedule.Monday, schedule.Tuesday,
                    schedule.Wednesday, schedule.Thursday, schedule.Friday, schedule.Saturday,
                    schedule.TimeOfDayUTC.AsTimeSpan(), schedule.RecurTime.AsTimeSpan(),
                    schedule.RecurBetweenStartUTC.AsTimeSpan(),
                    schedule.RecurBetweenEndUTC.AsTimeSpan(), oneTime: false, cancellationToken);
                return Ok(new { Success = true });
            }
            catch
            {
                _databaseFactory.MarkForRollback();
                return BadRequest("Unknown error, please try again");
            }
        }

        public record DeleteScheduleRequest(long ScheduleID);
        public record ReactivateScheduleRequest(long ScheduleID);
    }
}
