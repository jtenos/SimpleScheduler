using Microsoft.IdentityModel.Tokens;
using SimpleSchedulerApiModels.Reply.Login;
using SimpleSchedulerApiModels.Request.Login;
using SimpleSchedulerAppServices.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SimpleSchedulerBlazor.Server.ApiServices;

public static class LoginServiceMap
{
    private static async Task<GetAllUserEmailsReply> GetAllUserEmailsAsync(
        IUserManager userManager,
        IConfiguration config,
        GetAllUserEmailsRequest request)
    {
        return new GetAllUserEmailsReply(
            EmailAddresses: await userManager.GetAllUserEmailsAsync(allowLoginDropdown: config.AllowLoginDropdown())
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
        IConfiguration config,
        SubmitEmailRequest request)
    {
        if (!await userManager.LoginSubmitAsync(request.EmailAddress, config.WebUrl(), config.EnvironmentName()))
        {
            throw new KeyNotFoundException();
        }
        return new SubmitEmailReply();
    }

    private static async Task<ValidateEmailReply> ValidateEmailAsync(
        IUserManager userManager,
        IConfiguration config,
        ValidateEmailRequest request)
    {
        string emailAddress = await userManager.LoginValidateAsync(request.ValidationCode);
        string jwt = GenerateJwtToken(config, emailAddress);
        return new ValidateEmailReply(JwtToken: jwt);
    }

    public static void MapLoginService(this WebApplication app)
    {
        app.MapPost("/Login/GetAllUserEmails", GetAllUserEmailsAsync);
        app.MapPost("/Login/IsLoggedIn", IsLoggedInAsync);
        app.MapPost("/Login/SubmitEmail", SubmitEmailAsync);
        app.MapPost("/Login/ValidateEmail", ValidateEmailAsync);
    }

    private static string GenerateJwtToken(IConfiguration config, string emailAddress)
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
                    Convert.FromHexString(config.Jwt().Key)
                ),
                SecurityAlgorithms.HmacSha256Signature
            )
        };
        SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
