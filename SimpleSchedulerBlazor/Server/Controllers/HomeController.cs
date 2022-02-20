using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleSchedulerConfiguration.Models;
using SimpleSchedulerModels.ApiModels.Home;

namespace SimpleSchedulerBlazor.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class HomeController
    : ControllerBase
{
    private readonly AppSettings _appSettings;

    public HomeController(AppSettings appSettings)
    {
        _appSettings = appSettings;
    }

    [HttpPost("[action]")]
    [AllowAnonymous]
    public async Task<ActionResult<EnvironmentNameResponse>> EnvironmentName(
        EnvironmentNameRequest request, CancellationToken cancellationToken)
    {
        return await Task.FromResult(new EnvironmentNameResponse(_appSettings.EnvironmentName));
    }

    [HttpPost("[action]")]
    [AllowAnonymous]
    public async Task<ActionResult<HelloThereResponse>> HelloThere(
        HelloThereRequest request, CancellationToken cancellationToken)
    {
        return await Task.FromResult(new HelloThereResponse("Howdy"));
    }

    [HttpPost("[action]")]
    [AllowAnonymous]
    public async Task<ActionResult<UtcNowResponse>> UtcNow(
        UtcNowRequest request, CancellationToken cancellationToken)
    {
        return await Task.FromResult(new UtcNowResponse(DateTime.UtcNow.ToString("MMM dd yyyy HH\\:mm\\:ss")));
    }
}
