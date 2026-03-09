using MediatR;

namespace ProductService.API.DependencyInjection;

public static class ApplicationDependency
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
     
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(Application.AssemblyReference).Assembly);
        });

        return services;
    }
}
