using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using UserService.Application.Common.Exceptions;
using System.Security.Claims;
using System.Text;

namespace UserService.API.DependencyInjection;

public static class AuthenticationDependency
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection("Jwt");
        var keyString = jwtSection["Key"]
            ?? throw new ConfigurationMissingException("JWT Key");
        var issuer = jwtSection["Issuer"]
            ?? throw new ConfigurationMissingException("JWT Issuer");
        var audience = jwtSection["Audience"]
            ?? throw new ConfigurationMissingException("JWT Audience");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(keyString)
                ),
                RoleClaimType = ClaimTypes.Role,
                NameClaimType = ClaimTypes.NameIdentifier
            };
        });

        services.AddAuthorization();

        return services;
    }
}
