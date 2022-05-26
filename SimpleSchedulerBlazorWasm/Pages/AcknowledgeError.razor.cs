using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using SimpleSchedulerApiModels.Reply.Jobs;
using SimpleSchedulerApiModels.Request.Jobs;
using SimpleSchedulerServiceClient;

namespace SimpleSchedulerBlazorWasm.Pages;

partial class AcknowledgeError
{
    [Inject]
    private ServiceClient ServiceClient { get; set; } = default!;

    [Inject]
    private SweetAlertService Swal { get; set; } = default!;

    [Parameter]
    public Guid AcknowledgementCode { get; set; }

    protected override async Task OnInitializedAsync()
    {
        (Error? error, AcknowledgeErrorReply? reply) = await ServiceClient.PostAsync<AcknowledgeErrorRequest, AcknowledgeErrorReply>(
            "Jobs/AcknowledgeError",
            new AcknowledgeErrorRequest(AcknowledgementCode: AcknowledgementCode)
        );

        if (error is not null)
        {
            await Swal.FireAsync("Error", error.Message, SweetAlertIcon.Error);
            return;
        }

        await Swal.FireAsync(
            title: "Error Acknowledged",
            message: "The error has been successfully acknowledged",
            icon: SweetAlertIcon.Success
        );
    }
}
