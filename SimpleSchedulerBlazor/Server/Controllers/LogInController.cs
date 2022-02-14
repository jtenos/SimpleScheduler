using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using SimpleSchedulerBusiness;
using SimpleSchedulerEmail;
using SimpleSchedulerModels.Exceptions;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace SimpleSchedulerBlazor.Server.Controllers;

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
    public IActionResult ValidateAuthValue(string authValue)
    {
        return AuthValidation.IsValidAuth(_config, authValue)
            ? Ok()
            : Unauthorized();
    }

    [Route("[action]")]
    [HttpPost]
    public async Task<IActionResult> SubmitEmail([FromBody] SubmitEmailRequest request, CancellationToken cancellationToken)
    {
        if (!await _userManager.LoginSubmitAsync(request.EmailAddress, cancellationToken))
        {
            return StatusCode(401, new { Message = "User not found" });
        }

        return Ok(new { Success = true, Message = "Please check your email for a login link" });
    }

    [Route("[action]")]
    [HttpPost]
    public async Task<IActionResult> ValidateEmail([FromBody] ValidateEmailRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return (await _userManager.LoginValidateAsync(request.ValidationCode, cancellationToken))
                .Match(
                    emailAddress =>
                    {
                        AuthDef authObject = new(
                            EmailAddress: emailAddress,
                            ExpirationDate: DateTime.Now.AddMinutes(_config.GetValue<int>("ExpirationMinutes")),
                            AuthCode: _config["AuthCode"]
                        );

                        string authJson = JsonSerializer.Serialize(authObject);
                        byte[] authBytes = Encoding.UTF8.GetBytes(authJson);
                        byte[] encryptedAuth = Crypto.Encrypt(authBytes,
                            Convert.FromHexString(_config["CryptoKey"]),
                            Convert.FromHexString(_config["AuthKey"]));
                        string encryptedAuthHex = Convert.ToHexString(encryptedAuth);

                        return Ok(new { EmailAddress = emailAddress, Success = true, Auth = encryptedAuthHex });
                    }, notFound =>
                    {
                        return StatusCode(401, new { Message = "Invalid validation code" });
                    }, expired =>
                    {
                        return StatusCode(401, new { Message = "Validation code expired. Please try logging in again." });
                    }
                );
        }
        catch (Exception ex)
        {
            Trace.TraceError(ex.ToString());
            return Problem(ex.Message);
        }
    }

    [HttpGet("[action]")]
    public async Task<ImmutableArray<string>> GetAllUserEmails(CancellationToken cancellationToken)
        => await _userManager.GetAllUserEmailsAsync(cancellationToken);

    public record SubmitEmailRequest(string EmailAddress);
    public record ValidateEmailRequest(string ValidationCode);
}
