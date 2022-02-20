using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SimpleSchedulerBlazor.Server.Auth;

public class TokenService
    : ITokenService
{
    private static readonly TimeSpan _duration = TimeSpan.FromHours(8);

    public string BuildToken(string key, string issuer, string emailAddress)
    {
        Claim[] claims =
        {
            new (ClaimTypes.Email, emailAddress)
        };

        SymmetricSecurityKey securityKey = new(Encoding.UTF8.GetBytes(key));
        SigningCredentials credentials = new(securityKey, SecurityAlgorithms.HmacSha256Signature);
        JwtSecurityToken tokenDescriptor = new(
            issuer: issuer,
            audience: issuer,
            claims: claims,
            expires: DateTime.Now.Add(_duration),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }
}
