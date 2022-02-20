using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OneOf.Types;
using SimpleSchedulerAppServices.Interfaces;
using SimpleSchedulerConfiguration.Models;
using SimpleSchedulerModels.ApiModels.Login;
using SimpleSchedulerModels.ResultTypes;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SimpleSchedulerBlazor.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LoginController
    : ControllerBase
{
    private readonly AppSettings _appSettings;
    private readonly IUserManager _userManager;

    public LoginController(AppSettings appSettings, IUserManager userManager)
    {
        _appSettings = appSettings;
        _userManager = userManager;
    }

    [HttpPost("[action]")]
    public async Task<ActionResult<IsLoggedInResponse>> IsLoggedIn(
        IsLoggedInRequest request, CancellationToken cancellationToken)
    {
        return await Task.FromResult(new IsLoggedInResponse(HttpContext.User != null));
    }

    [HttpPost("[action]")]
    public async Task<ActionResult<GetAllUserEmailsResponse>> GetAllUserEmails(
        GetAllUserEmailsRequest request, CancellationToken cancellationToken)
    {
        return new GetAllUserEmailsResponse(await _userManager.GetAllUserEmailsAsync(cancellationToken));
    }

    [HttpPost("[action]")]
    public async Task<ActionResult<SubmitEmailResponse>> SubmitEmail(
        SubmitEmailRequest request, CancellationToken cancellationToken)
    {
        return await _userManager.LoginSubmitAsync(request.EmailAddress, cancellationToken)
            ? new SubmitEmailResponse()
            : NotFound();
    }

    [HttpPost("[action]")]
    public async Task<ActionResult<ValidateEmailResponse>> ValidateEmail(
        ValidateEmailRequest request, CancellationToken cancellationToken)
    {
        return (await _userManager.LoginValidateAsync(request.ValidationCode, cancellationToken))
            .Match<ActionResult<ValidateEmailResponse>>(
                (string emailAddress) =>
                {
                    string jwt = GenerateJwtToken(emailAddress);
                    return new ValidateEmailResponse(jwt);
                }, (NotFound notFound) =>
                {
                    return NotFound();
                }, (Expired expired) =>
                {
                    return BadRequest("Validation code expired");
                }
            )!;
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
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(
                    Convert.FromHexString(_appSettings.Jwt.Key)
                ),
                SecurityAlgorithms.HmacSha256Signature
            )
        };
        SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
