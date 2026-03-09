using UserService.Application.Common.Interfaces;
using UserService.Application.Contracts;
using UserService.Infrastructure.Services;
using UserService.Infrastructure.Settings;
using UserService.API.Services;

namespace UserService.API.DependencyInjection;

public static class InfrastructureDependency
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtService, JwtTokenGenerator>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IOtpService, OtpService>();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
        services.Configure<OtpSettings>(configuration.GetSection("OtpSettings"));

        services.AddMemoryCache();

        return services;
    }
}
