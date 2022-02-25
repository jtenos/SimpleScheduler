using SimpleSchedulerApiModels.Reply.Schedules;
using SimpleSchedulerApiModels.Request.Schedules;
using System.ServiceModel;

namespace SimpleScheduler.Blazor.Shared.ServiceContracts;

[ServiceContract(Name = nameof(ISchedulesService))]
public interface ISchedulesService
{
    Task<CreateScheduleReply> CreateScheduleAsync(CreateScheduleRequest request);
    Task<DeleteScheduleReply> DeleteScheduleAsync(DeleteScheduleRequest request);
    Task<GetAllSchedulesReply> GetAllSchedulesAsync(GetAllSchedulesRequest request);
    Task<GetScheduleReply> GetScheduleAsync(GetScheduleRequest request);
    Task<ReactivateScheduleReply> ReactivateScheduleAsync(ReactivateScheduleRequest request);
    Task<UpdateScheduleReply> UpdateScheduleAsync(UpdateScheduleRequest request);
}
