using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ABAPpartment.Application.Interfaces;
using ABAPpartment.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ABAPpartment.Infrastructure.Services;

public class JwtService : IJwtService
{
    private readonly string _secret;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expiryMinutes;

    public JwtService(IConfiguration config)
    {
        _secret = config["JwtSettings:Secret"] ?? throw new InvalidOperationException("JwtSettings:Secret no configurado.");
        _issuer = config["JwtSettings:Issuer"] ?? "ABAPpartment.API";
        _audience = config["JwtSettings:Audience"] ?? "ABAPpartment.Client";
        _expiryMinutes = int.Parse(config["JwtSettings:ExpiryMinutes"] ?? "60");
    }

    public (string Token, DateTime ExpiresAt) GenerateToken(User user)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_expiryMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,   user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role,               user.Role),
            new Claim("firstName",                   user.FirstName),
            new Claim("lastName",                    user.LastName),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}