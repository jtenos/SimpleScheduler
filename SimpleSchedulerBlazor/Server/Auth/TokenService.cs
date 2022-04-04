using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SimpleSchedulerBlazor.Server.Auth;

public class TokenService : ITokenService
{
    private static readonly TimeSpan _expirationDuration = TimeSpan.FromHours(12);

    string ITokenService.BuildToken(IConfiguration config, string emailAddress)
    {
        var (issuer, audience, keyHex) = config.Jwt();
        byte[] key = Convert.FromHexString(keyHex);
        var claims = new[]
        {
            new Claim(ClaimTypes.Email, emailAddress),
        };

        SymmetricSecurityKey securityKey = new(key);
        SigningCredentials credentials = new(securityKey, SecurityAlgorithms.HmacSha256Signature);
        JwtSecurityToken tokenDescriptor = new(issuer, audience, claims,
            expires: DateTime.Now.Add(_expirationDuration), signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }
}