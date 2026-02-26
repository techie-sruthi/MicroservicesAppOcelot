using MediatR;

namespace UserService.API.DependencyInjection;

public static class ApplicationDependency
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // MediatR for CQRS
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(Application.AssemblyReference).Assembly);
        });

        return services;
    }
}
