using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using SimpleSchedulerBusiness;

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
            return Ok(await Task.FromResult(new{
                Message = "Howdy"
            }));
        }
    }
}