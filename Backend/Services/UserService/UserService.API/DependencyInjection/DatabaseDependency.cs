using Microsoft.EntityFrameworkCore;
using UserService.Application.Common.Interfaces;
using UserService.Infrastructure.Persistence;

namespace UserService.API.DependencyInjection;

public static class DatabaseDependency
{
    public static IServiceCollection AddDatabaseServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<UserDbContextScaffolded>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUserDbContext>(provider =>
            provider.GetRequiredService<UserDbContextScaffolded>());

        return services;
    }
}
