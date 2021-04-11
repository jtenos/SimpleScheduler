using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using SimpleSchedulerBusiness;
using System;
using System.Text.Json;

namespace SimpleSchedulerAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class HelloController
        : ControllerBase
    {
        private readonly IUserManager _userManager;

        public HelloController(IUserManager userManager) => _userManager = userManager;

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
    }
}