using Microsoft.IdentityModel.Tokens;
using SimpleSchedulerApiModels.Reply.Login;
using SimpleSchedulerApiModels.Request.Login;
using SimpleSchedulerAppServices.Interfaces;
using SimpleSchedulerConfiguration.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SimpleSchedulerBlazor.Server.ApiServices;

public static class LoginServiceMap
{
    public static void MapLoginService(this WebApplication app)
    {
        app.MapPost("/Login/GetAllUserEmails",
            async (
                IUserManager userManager,
                GetAllUserEmailsRequest request
            ) =>
            {
                return new GetAllUserEmailsReply(
                    emailAddresses: await userManager.GetAllUserEmailsAsync()
                );
            });

        app.MapPost("/Login/IsLoggedIn",
            (
                IHttpContextAccessor httpContextAccessor,
                IsLoggedInRequest request
            ) =>
            {
                return new IsLoggedInReply(
                    isLoggedIn: httpContextAccessor.HttpContext?.User is not null
                );
            });

        app.MapPost("/Login/SubmitEmail",
            async (
                IUserManager userManager,
                SubmitEmailRequest request
            ) =>
            {
                if (!await userManager.LoginSubmitAsync(request.EmailAddress))
                {
                    return Results.NotFound("Email address not found");
                }
                return Results.Ok(new SubmitEmailReply());
            });

        app.MapPost("/Login/ValidateEmail",
            async (
                IUserManager userManager,
                AppSettings appSettings,
                ValidateEmailRequest request
            ) =>
            {
                string emailAddress = await userManager.LoginValidateAsync(request.ValidationCode);
                string jwt = GenerateJwtToken(appSettings, emailAddress);
                return new ValidateEmailReply(jwtToken: jwt);
            });
    }

    private static string GenerateJwtToken(AppSettings appSettings, string emailAddress)
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
                    Convert.FromHexString(appSettings.Jwt.Key)
                ),
                SecurityAlgorithms.HmacSha256Signature
            )
        };
        SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
