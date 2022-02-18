using Microsoft.AspNetCore.Mvc;
using SimpleSchedulerModels.Configuration;

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

    [HttpGet]
    [Route("[action]")]
    public ActionResult<string> EnvironmentName()
    {
        return Ok(_appSettings.EnvironmentName);
    }

    [HttpGet]
    [Route("[action]")]
    public ActionResult<string> HelloThere()
    {
        return Ok("Howdy");
    }

    [HttpGet]
    [Route("[action]")]
    public ActionResult<string> GetUtcNow()
    {
        return Ok(DateTime.UtcNow.ToString("MMM dd yyyy HH\\:mm\\:ss"));
    }
}
