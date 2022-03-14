using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SimpleSchedulerBlazor.Server.Auth;

public class TokenService : ITokenService
{
    private TimeSpan ExpiryDuration = new TimeSpan(0, 30, 0);
    string ITokenService.BuildToken(IConfiguration config, string emailAddress)
    {
        var jwt = config.Jwt();
        byte[] key = Convert.FromHexString(jwt.Key);
        string issuer = jwt.Issuer;
        string audience = jwt.Audience;
        var claims = new[]
        {
            new Claim(ClaimTypes.Email, emailAddress),
        };

        var securityKey = new SymmetricSecurityKey(key);
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);
        var tokenDescriptor = new JwtSecurityToken(issuer, audience, claims,
            expires: DateTime.Now.Add(ExpiryDuration), signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }
}