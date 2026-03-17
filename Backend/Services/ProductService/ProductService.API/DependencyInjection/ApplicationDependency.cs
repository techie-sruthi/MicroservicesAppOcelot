using MediatR;
using ProductService.Application;

namespace ProductService.API.DependencyInjection;

public static class ApplicationDependency
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
     
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(IAssemblyReference).Assembly));
        
        return services;
    }
}
