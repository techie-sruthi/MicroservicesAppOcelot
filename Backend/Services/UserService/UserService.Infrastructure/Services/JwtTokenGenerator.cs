using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using UserService.Application.Common.Exceptions;
using UserService.Application.Common.Interfaces;

namespace UserService.Infrastructure.Services;

public class JwtTokenGenerator : IJwtService
{
    private readonly string _keyString;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expiryMinutes;

    public JwtTokenGenerator(IConfiguration configuration)
    {
        var jwt = configuration.GetSection("Jwt");
        _keyString = jwt["Key"]
            ?? throw new ConfigurationMissingException("JWT Key");
        _issuer = jwt["Issuer"]
            ?? throw new ConfigurationMissingException("JWT Issuer");
        _audience = jwt["Audience"]
            ?? throw new ConfigurationMissingException("JWT Audience");
        _expiryMinutes = int.Parse(jwt["ExpiryMinutes"]
            ?? throw new ConfigurationMissingException("JWT ExpiryMinutes"));
    }

    public string GenerateToken(int userId, string email, string role)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_keyString)
        );

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim("role", role)
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_expiryMinutes),
            signingCredentials: new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256
            )
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return tokenString;
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}
