using SimpleSchedulerApiModels.Reply.Login;
using SimpleSchedulerApiModels.Request.Login;
using System.ServiceModel;

namespace SimpleScheduler.Blazor.Shared.ServiceContracts;

[ServiceContract(Name = nameof(ILoginService))]
public interface ILoginService
{
    Task<GetAllUserEmailsReply> GetAllUserEmailsAsync(GetAllUserEmailsRequest request);
    Task<IsLoggedInReply> IsLoggedInAsync(IsLoggedInRequest request);
    Task<SubmitEmailReply> SubmitEmailAsync(SubmitEmailRequest request);
    Task<ValidateEmailReply> ValidateEmailAsync(ValidateEmailRequest request);
}
