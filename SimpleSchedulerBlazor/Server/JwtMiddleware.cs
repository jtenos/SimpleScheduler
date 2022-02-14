using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SimpleSchedulerBlazor.Server;

/// <summary>
/// Based on https://jasonwatmore.com/post/2020/08/13/blazor-webassembly-jwt-authentication-example-tutorial
/// </summary>
public class JwtMiddleware
{
    private readonly RequestDelegate _next;
    private readonly byte[] _jwtSecret;

    public JwtMiddleware(RequestDelegate next, IConfiguration config)
    {
        _next = next;
        _jwtSecret = Convert.FromHexString(config["JwtSecret"]);
    }

    public async Task Invoke(HttpContext context)
    {
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

        if (token is not null)
        {
            try
            {
                JwtSecurityTokenHandler tokenHandler = new();
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(_jwtSecret),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                JwtSecurityToken jwtToken = (JwtSecurityToken)validatedToken;
                context.Items["EmailAddress"] = jwtToken.Claims.First(x => x.Type == ClaimTypes.Email).Value;
            }
            catch
            {
            }
        }

        await _next(context);
    }
}
