using Microsoft.IdentityModel.Tokens;
using SimpleSchedulerBusiness;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SimpleSchedulerBlazor.Server.ApiHandlers;

public static class LoginApiExtensions
{
    public static WebApplicationBuilder AddLoginApi(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<LoginApi>();
        return builder;
    }

    public static WebApplication MapLoginApi(this WebApplication app)
    {
        app.Map("/Login/GetAllUserEmails", async (LoginApi loginApi, CancellationToken cancellationToken)
            => await loginApi.GetAllUserEmails(cancellationToken));

        app.Map("/Login/SubmitEmail", async (SubmitEmailRequest request, LoginApi loginApi, CancellationToken cancellationToken)
            => await loginApi.SubmitEmailAsync(request, cancellationToken));

        app.Map("/Login/ValidateEmail", async (ValidateEmailRequest request, LoginApi loginApi, CancellationToken cancellationToken)
            => await loginApi.ValidateEmailAsync(request, cancellationToken));

        return app;
    }
}

internal class LoginApi
{
    private readonly IUserManager _userManager;
    private readonly byte[] _jwtSecret;

    public LoginApi(IUserManager userManager, IConfiguration config)
    {
        _userManager = userManager;
        _jwtSecret = Convert.FromHexString(config["JwtSecret"]);
    }

    public async Task<IResult> GetAllUserEmails(CancellationToken cancellationToken)
    {
        return Results.Ok(await _userManager.GetAllUserEmailsAsync(cancellationToken));
    }

    public async Task<IResult> SubmitEmailAsync(SubmitEmailRequest request, CancellationToken cancellationToken)
    {
        if (!await _userManager.LoginSubmitAsync(request.EmailAddress, cancellationToken))
        {
            return Results.NotFound(new { Success = false, Message = "User not found" });
        }

        return Results.Ok(new { Success = true, Message = "Please check your email for a login link" });
    }

    public async Task<IResult> ValidateEmailAsync(ValidateEmailRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return (await _userManager.LoginValidateAsync(request.ValidationCode, cancellationToken))
                .Match(
                    emailAddress =>
                    {
                        return Results.Ok(new
                        {
                            JwtToken = GenerateJwtToken(emailAddress)
                        });
                    }, notFound =>
                    {
                        return Results.BadRequest("Validation code not found");
                    }, expired =>
                    {
                        return Results.BadRequest("Validation code expired. Please try logging in again.");
                    }
                );
        }
        catch (Exception ex)
        {
            Trace.TraceError(ex.ToString());
            return Results.Problem(ex.Message);
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

internal record SubmitEmailRequest(string EmailAddress);
internal record ValidateEmailRequest(string ValidationCode);
