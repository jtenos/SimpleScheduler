using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OneOf.Types;
using SimpleSchedulerBusiness;
using SimpleSchedulerModels.ApiModels;
using SimpleSchedulerModels.ResultTypes;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SimpleSchedulerBlazor.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LoginController
    : ControllerBase
{
    private readonly IUserManager _userManager;
    private readonly byte[] _jwtSecret;

    public LoginController(IUserManager userManager, IConfiguration config)
    {
        _userManager = userManager;
        _jwtSecret = Convert.FromHexString(config["JwtSecret"]);
    }

    [HttpGet]
    [Route("[action]")]
    public async Task<ActionResult<ImmutableArray<string>>> GetAllUserEmails(CancellationToken cancellationToken)
    {
        return Ok(await _userManager.GetAllUserEmailsAsync(cancellationToken));
    }

    [HttpPost]
    [Route("[action]")]
    public async Task<ActionResult<SubmitEmailResponse>> SubmitEmailAsync(SubmitEmailRequest request, CancellationToken cancellationToken)
    {
        if (!await _userManager.LoginSubmitAsync(request.EmailAddress, cancellationToken))
        {
            return NotFound(new SubmitEmailResponse(Success: false, Message: "User not found"));
        }

        return Ok(new SubmitEmailResponse(Success: true, Message: "Please check your email for a login link"));
    }

    [HttpPost]
    [Route("[action]")]
    public async Task<IActionResult> ValidateEmailAsync(ValidateEmailRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return (await _userManager.LoginValidateAsync(request.ValidationCode, cancellationToken))
                .Match<IActionResult>(
                    (string emailAddress) =>
                    {
                        return Ok(new { JwtToken = GenerateJwtToken(emailAddress) });
                    }, (NotFound notFound) =>
                    {
                        return BadRequest("Validation code not found");
                    }, (Expired expired) =>
                    {
                        return BadRequest("Validation code expired. Please try logging in again.");
                    }
                );
        }
        catch (Exception ex)
        {
            Trace.TraceError(ex.ToString());
            return Problem(ex.Message);
        }
    }

    private string GenerateJwtToken(string emailAddress)
    {
        JwtSecurityTokenHandler tokenHandler = new();
        SecurityTokenDescriptor? tokenDescriptor = new()
        {
            Subject = new ClaimsIdentity(new[] {
                new Claim(ClaimTypes.Email, emailAddress)
            }),
            Expires = DateTime.UtcNow.AddDays(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(_jwtSecret), SecurityAlgorithms.HmacSha256Signature)
        };
        SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
