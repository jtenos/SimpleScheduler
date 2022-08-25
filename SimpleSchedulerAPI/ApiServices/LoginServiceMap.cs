using Microsoft.AspNetCore.Authorization;
using SimpleSchedulerAPI.Auth;
using SimpleSchedulerApiModels.Reply.Login;
using SimpleSchedulerApiModels.Request.Login;
using SimpleSchedulerAppServices.Interfaces;

namespace SimpleSchedulerAPI.ApiServices;

public static class LoginServiceMap
{
    [AllowAnonymous]
    private static async Task<GetAllUserEmailsReply> GetAllUserEmailsAsync(
        IUserManager userManager,
        IConfiguration config,
        GetAllUserEmailsRequest request)
    {
        return new GetAllUserEmailsReply(
            EmailAddresses: await userManager.GetAllUserEmailsAsync(allowLoginDropdown: config.AllowLoginDropdown())
        );
    }

    [AllowAnonymous]
    private static Task<IsLoggedInReply> IsLoggedInAsync(
        IHttpContextAccessor httpContextAccessor,
        IsLoggedInRequest request)
    {
        return Task.FromResult(new IsLoggedInReply(
            IsLoggedIn: httpContextAccessor.HttpContext?.User is not null
        ));
    }

    [AllowAnonymous]
    private static async Task<SubmitEmailReply> SubmitEmailAsync(
        IUserManager userManager,
        IConfiguration config,
        SubmitEmailRequest request)
    {
        try
        {
            bool success = await userManager.LoginSubmitAsync(request.EmailAddress, config.WebUrl());
		    return new SubmitEmailReply(Success: success);
		}
        catch
        {
            return new SubmitEmailReply(Success: false);
        }
    }

    [AllowAnonymous]
    private static async Task<ValidateEmailReply> ValidateEmailAsync(
        IUserManager userManager,
        IConfiguration config,
        ITokenService tokenService,
        ValidateEmailRequest request)
    {
        string emailAddress = await userManager.LoginValidateAsync(
            validationCode: request.ValidationCode,
            internalSecretAuthKey: config.InternalSecretAuthKey()
        );

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
