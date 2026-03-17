using ProductService.API.DependencyInjection;
using ProductService.API.Middleware;
using ProductService.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = Directory.GetCurrentDirectory()
});

var port = Environment.GetEnvironmentVariable("PRODUCTSERVICE_PORT")
    ?? builder.Configuration["ServiceSettings:Port"]
    ?? "5050";

if (builder.Environment.IsProduction())
    builder.WebHost.UseUrls($"https://*:{port}");
else
    builder.WebHost.UseUrls($"http://*:{port}");

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.WriteIndented = false;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddCorsPolicy(builder.Configuration);
MongoMappings.RegisterMappings();

var app = builder.Build();

app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAngular");

app.UseMiddleware<ExceptionMiddleware>();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();