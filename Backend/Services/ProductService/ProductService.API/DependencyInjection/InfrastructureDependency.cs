using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ProductService.Application.Common.Interfaces;
using ProductService.Infrastructure.Data;
using ProductService.Infrastructure.FileStorage;
using ProductService.Infrastructure.Repositories;

namespace ProductService.API.DependencyInjection;

public static class InfrastructureDependency
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Bind MongoDB settings
        services.Configure<MongoDbSettings>(
            configuration.GetSection("MongoDB"));

        services.AddSingleton<IMongoClient>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
            return new MongoClient(settings.ConnectionString);
        });

        // Register Repository as Scoped
        services.AddScoped<IProductRepository, ProductRepository>();

        // Configure MinIO
        services.Configure<MinIOSettings>(
            configuration.GetSection("FileStorage:MinIO"));

        services.AddScoped<IFileStorageService, MinIOFileStorageService>();

        return services;
    }
}
