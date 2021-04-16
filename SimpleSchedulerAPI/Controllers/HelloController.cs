using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using SimpleSchedulerBusiness;
using System;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace SimpleSchedulerAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class HelloController
        : ControllerBase
    {
        private readonly IUserManager _userManager;
        private readonly IConfiguration _config;

        public HelloController(IUserManager userManager, IConfiguration config)
            => (_userManager, _config) = (userManager, config);

        [Route("[action]")]
        [HttpGet]
        public async Task<IActionResult> HelloThere(CancellationToken cancellationToken)
        {
            return Ok(await Task.FromResult(new
            {
                Message = "Howdy"
            }));
        }

        [HttpGet("[action]")]
        [ResponseCache(NoStore = true)]
        public IActionResult GetUtcNow() => Ok(JsonSerializer.Serialize(DateTime.UtcNow.ToString("MMM dd yyyy HH\\:mm\\:ss")));

        [HttpGet("[action]")]
        public IActionResult GetEnvironmentName() => Ok(JsonSerializer.Serialize(_config.GetValue<string>("EnvironmentName")));
    }
}