using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SimpleSchedulerBusiness;
using SimpleSchedulerEmail;
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
        private readonly IEmailer _emailer;
        private readonly IConfiguration _config;
        public LogInController(IUserManager userManager, IEmailer emailer, IConfiguration config) 
            => (_userManager, _emailer, _config) = (userManager, emailer, config);

        [Route("[action]")]
        [HttpPost]
        public async Task<IActionResult> SubmitEmail([FromBody]SubmitEmailRequest request, CancellationToken cancellationToken)
        {
            var result = await _userManager.LoginSubmitAsync(request.EmailAddress, cancellationToken);
            if (!result.EmailFound)
            {
                return StatusCode(401, new { Message = "User not found" });
            }
            string url = $"{_config["WebUrl"]}/validate-user/{result.ValidationKey}";
            await _emailer.SendEmailAsync(new[] { request.EmailAddress },
                $"Scheduler ({_config["EnvironmentName"]}) Log In",
                $"<a href='{url}' target=_blank>Click here to log in</a>",
                cancellationToken);
            return Ok(new { Success = true, Message = "Please check your email for a login link" });
        }

        [Route("[action]")]
        [HttpPost]
        public async Task<IActionResult> ValidateEmail([FromBody]ValidateEmailRequest request, CancellationToken cancellationToken)
        {
            try
            {
                string emailAddress = await _userManager.LoginValidateAsync(Guid.Parse(request.ValidationCode), cancellationToken);
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
        public record ValidateEmailRequest(string ValidationCode);
    }
}
