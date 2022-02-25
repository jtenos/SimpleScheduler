using Grpc.Core;
using Microsoft.IdentityModel.Tokens;
using OneOf.Types;
using SimpleScheduler.Blazor.Shared.ServiceContracts;
using SimpleSchedulerApiModels.Reply.Login;
using SimpleSchedulerApiModels.Request.Login;
using SimpleSchedulerAppServices.Interfaces;
using SimpleSchedulerConfiguration.Models;
using SimpleSchedulerModels.ResultTypes;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SimpleSchedulerBlazor.Server.GrpcServices;

public class LoginService
    : ILoginService
{
    private readonly AppSettings _appSettings;
    private readonly IUserManager _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LoginService(AppSettings appSettings, IUserManager userManager, IHttpContextAccessor httpContextAccessor)
    {
        _appSettings = appSettings;
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
    }

    async Task<GetAllUserEmailsReply> ILoginService.GetAllUserEmailsAsync(GetAllUserEmailsRequest request)
    {
        return new GetAllUserEmailsReply(
            emailAddresses: await _userManager.GetAllUserEmailsAsync()
        );
    }

    Task<IsLoggedInReply> ILoginService.IsLoggedInAsync(IsLoggedInRequest request)
    {
        return Task.FromResult(new IsLoggedInReply(
            isLoggedIn: _httpContextAccessor.HttpContext?.User is not null
        ));
    }

    async Task<SubmitEmailReply> ILoginService.SubmitEmailAsync(SubmitEmailRequest request)
    {
        try
        {
            if (!await _userManager.LoginSubmitAsync(request.EmailAddress))
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Email address not found"));
            }
        }
        catch (Exception ex)
        {
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
        return new SubmitEmailReply();
    }

    async Task<ValidateEmailReply> ILoginService.ValidateEmailAsync(ValidateEmailRequest request)
    {
        return (await _userManager.LoginValidateAsync(request.ValidationCode))
            .Match<ValidateEmailReply>((string emailAddress) =>
            {
                try
                {
                    string jwt = GenerateJwtToken(emailAddress);
                    return new(jwtToken: jwt);
                }
                catch (Exception ex)
                {
                    throw new RpcException(new Status(StatusCode.Internal, $"{ex.GetType().Name}: {ex.Message}"));
                }
            }, (NotFound notFound) =>
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Validation code not found"));
            }, (Expired expired) =>
            {
                throw new RpcException(new Status(StatusCode.OutOfRange, "Validation code expired"));
            });
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
