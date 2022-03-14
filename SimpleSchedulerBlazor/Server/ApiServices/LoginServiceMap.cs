using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using SimpleSchedulerApiModels.Reply.Login;
using SimpleSchedulerApiModels.Request.Login;
using SimpleSchedulerAppServices.Interfaces;
using SimpleSchedulerBlazor.Server.Auth;
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

    [AllowAnonymous]
    private static async Task<ValidateEmailReply> ValidateEmailAsync(
        IUserManager userManager,
        IConfiguration config,
        ITokenService tokenService,
        ValidateEmailRequest request)
    {
        string emailAddress = await userManager.LoginValidateAsync(request.ValidationCode);

        byte[] key = Convert.FromHexString(config.Jwt().Key);
        string token = tokenService.BuildToken(config, emailAddress);

        return new ValidateEmailReply(JwtToken: token);
    }

    public static void MapLoginService(this WebApplication app)
    {
        app.MapPost("/Login/GetAllUserEmails", GetAllUserEmailsAsync);
        app.MapPost("/Login/IsLoggedIn", IsLoggedInAsync);
        app.MapPost("/Login/SubmitEmail", SubmitEmailAsync);
        app.MapPost("/Login/ValidateEmail", ValidateEmailAsync);
    }
}
