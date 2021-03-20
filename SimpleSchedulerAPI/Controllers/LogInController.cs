using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using SimpleSchedulerBusiness;
using SimpleSchedulerModels.Exceptions;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleSchedulerAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class LogInController : ControllerBase
    {
        private readonly IUserManager _userManager;
        public LogInController(IUserManager userManager) => _userManager = userManager;

        [Route("[action]")]
        [HttpPost]
        public async Task<IActionResult> SubmitEmail([FromBody]SubmitEmailRequest request, CancellationToken cancellationToken)
        {
            var result = await _userManager.LoginSubmitAsync(request.EmailAddress, cancellationToken);
            if (!result.EmailFound)
            {
                return StatusCode(401, new { Message = "User not found" });
            }
            return Ok(new { Message = "Please check your email for a login link" });
        }

        [Route("[action]")]
        [HttpGet]
        public async Task<IActionResult> ValidateEmail([FromQuery]ValidateEmailRequest request, CancellationToken cancellationToken)
        {
            try
            {
                string emailAddress = await _userManager.LoginValidateAsync(request.ValidationCode, cancellationToken);
                var claimsIdentity = new ClaimsIdentity(new[]
                {
                    new Claim("IsAuthenticated", "1"),
                    new Claim(ClaimTypes.Email, emailAddress)
                }, "Cookies");
                var principal = new ClaimsPrincipal(claimsIdentity);
                await HttpContext.SignInAsync(principal);
                return Ok();
            }
            catch (InvalidValidationKeyException)
            {
                return StatusCode(401, new { Message = "Invalid validation code" });
            }
            catch (ValidationKeyExpiredException)
            {
                return StatusCode(401, new { Message = "Validation code expired. Please try logging in again." });
            }
        }

        public record SubmitEmailRequest(string EmailAddress);
        public record ValidateEmailRequest(Guid ValidationCode);
    }
}
