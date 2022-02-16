using Microsoft.AspNetCore.Mvc;

namespace SimpleSchedulerBlazor.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class HomeController 
    : ControllerBase
{
    private readonly IConfiguration _config;

    public HomeController(IConfiguration config)
    {
        _config = config;
    }

    [HttpGet]
    [Route("[action]")]
    public ActionResult<string> EnvironmentName()
    {
        return Ok(_config["EnvironmentName"]);
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
