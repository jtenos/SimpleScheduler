using Grpc.Core;
using Microsoft.IdentityModel.Tokens;
using OneOf.Types;
using SimpleSchedulerAppServices.Interfaces;
using SimpleSchedulerBlazor.ProtocolBuffers.Messages.Login;
using SimpleSchedulerConfiguration.Models;
using SimpleSchedulerModels.ResultTypes;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using static SimpleSchedulerBlazor.ProtocolBuffers.Services.LoginService;

namespace SimpleSchedulerBlazor.Server.GrpcServices;

public class LoginService
    : LoginServiceBase
{
    private readonly AppSettings _appSettings;
    private readonly IUserManager _userManager;

    public LoginService(AppSettings appSettings, IUserManager userManager)
    {
        _appSettings = appSettings;
        _userManager = userManager;
    }

    public override async Task<GetAllUserEmailsReply> GetAllUserEmails(GetAllUserEmailsRequest request, ServerCallContext context)
    {
        return new GetAllUserEmailsReply(
            emailAddresses: await _userManager.GetAllUserEmailsAsync(context.CancellationToken)
        );
    }

    public override Task<IsLoggedInReply> IsLoggedIn(IsLoggedInRequest request, ServerCallContext context)
    {
        return Task.FromResult(new IsLoggedInReply(
            isLoggedIn: context.GetHttpContext()?.User is not null
        ));
    }

    public override async Task<SubmitEmailReply> SubmitEmail(SubmitEmailRequest request, ServerCallContext context)
    {
        try
        {
            if (!await _userManager.LoginSubmitAsync(request.EmailAddress, context.CancellationToken))
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

    public override async Task<ValidateEmailReply> ValidateEmail(ValidateEmailRequest request, ServerCallContext context)
    {
        return (await _userManager.LoginValidateAsync(Guid.Parse(request.ValidationCode), context.CancellationToken))
            .Match<ValidateEmailReply>((string emailAddress) =>
            {
                string jwt = GenerateJwtToken(emailAddress);
                return new(jwtToken: jwt);
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
