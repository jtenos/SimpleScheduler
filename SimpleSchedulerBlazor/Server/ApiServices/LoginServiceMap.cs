using Microsoft.IdentityModel.Tokens;
using SimpleSchedulerApiModels.Reply.Login;
using SimpleSchedulerApiModels.Request.Login;
using SimpleSchedulerAppServices.Interfaces;
using SimpleSchedulerBlazor.Server.Config;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SimpleSchedulerBlazor.Server.ApiServices;

public static class LoginServiceMap
{
    private static async Task<GetAllUserEmailsReply> GetAllUserEmailsAsync(
        IUserManager userManager,
        AppSettings appSettings,
        GetAllUserEmailsRequest request)
    {
        return new GetAllUserEmailsReply(
            EmailAddresses: await userManager.GetAllUserEmailsAsync(allowLoginDropdown: appSettings.AllowLoginDropDown)
        );
    }

    private static Task<IsLoggedInReply> IsLoggedInAsync(
        IHttpContextAccessor httpContextAccessor,
        IsLoggedInRequest request)
    {
        return Task.FromResult(new IsLoggedInReply(
            IsLoggedIn: httpContextAccessor.HttpContext?.User is not null
        ));
    }

    private static async Task<SubmitEmailReply> SubmitEmailAsync(
        IUserManager userManager,
        AppSettings appSettings,
        SubmitEmailRequest request)
    {
        if (!await userManager.LoginSubmitAsync(request.EmailAddress, appSettings.WebUrl, appSettings.EnvironmentName))
        {
            throw new KeyNotFoundException();
        }
        return new SubmitEmailReply();
    }

    private static async Task<ValidateEmailReply> ValidateEmailAsync(
        IUserManager userManager,
        AppSettings appSettings,
        ValidateEmailRequest request)
    {
        string emailAddress = await userManager.LoginValidateAsync(request.ValidationCode);
        string jwt = GenerateJwtToken(appSettings, emailAddress);
        return new ValidateEmailReply(JwtToken: jwt);
    }

    public static void MapLoginService(this WebApplication app)
    {
        app.MapPost("/Login/GetAllUserEmails", GetAllUserEmailsAsync);
        app.MapPost("/Login/IsLoggedIn", IsLoggedInAsync);
        app.MapPost("/Login/SubmitEmail", SubmitEmailAsync);
        app.MapPost("/Login/ValidateEmail", ValidateEmailAsync);
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
